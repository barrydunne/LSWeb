using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudWatchLogs;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.FilterLogEvents;

internal sealed partial class FilterLogEventsQueryHandler
    : IQueryHandler<FilterLogEventsQuery, FilterLogEventsQueryResult>
{
    private readonly ICloudWatchLogsClient _client;
    private readonly ILogger _logger;

    public FilterLogEventsQueryHandler(ICloudWatchLogsClient client, ILogger<FilterLogEventsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<FilterLogEventsQueryResult>> Handle(
        FilterLogEventsQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.LogGroupName);
        var events = await _client.FilterLogEventsAsync(
            request.LogGroupName, request.FilterPattern, request.StartTime, request.Limit, cancellationToken);
        LogHandled(events.IsSuccess);

        if (!events.IsSuccess)
        {
            Result<FilterLogEventsQueryResult> failure = events.Error!.Value;
            return failure;
        }

        return new FilterLogEventsQueryResult(events.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Filtering log events for {LogGroupName}.")]
    private partial void LogHandling(string logGroupName);

    [LoggerMessage(LogLevel.Trace, "CloudWatch log event filter handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
