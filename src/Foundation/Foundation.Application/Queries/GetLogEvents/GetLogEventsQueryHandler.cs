using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudWatchLogs;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetLogEvents;

internal sealed partial class GetLogEventsQueryHandler
    : IQueryHandler<GetLogEventsQuery, GetLogEventsQueryResult>
{
    private readonly ICloudWatchLogsClient _client;
    private readonly ILogger _logger;

    public GetLogEventsQueryHandler(ICloudWatchLogsClient client, ILogger<GetLogEventsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetLogEventsQueryResult>> Handle(
        GetLogEventsQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.LogGroupName, request.LogStreamName);
        var events = await _client.GetLogEventsAsync(
            request.LogGroupName, request.LogStreamName, request.Limit, cancellationToken);
        LogHandled(events.IsSuccess);

        if (!events.IsSuccess)
        {
            Result<GetLogEventsQueryResult> failure = events.Error!.Value;
            return failure;
        }

        return new GetLogEventsQueryResult(events.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Reading log events for {LogGroupName}/{LogStreamName}.")]
    private partial void LogHandling(string logGroupName, string logStreamName);

    [LoggerMessage(LogLevel.Trace, "CloudWatch log event read handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
