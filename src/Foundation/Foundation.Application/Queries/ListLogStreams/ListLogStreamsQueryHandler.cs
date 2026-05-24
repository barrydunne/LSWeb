using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudWatchLogs;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListLogStreams;

internal sealed partial class ListLogStreamsQueryHandler
    : IQueryHandler<ListLogStreamsQuery, ListLogStreamsQueryResult>
{
    private readonly ICloudWatchLogsClient _client;
    private readonly ILogger _logger;

    public ListLogStreamsQueryHandler(ICloudWatchLogsClient client, ILogger<ListLogStreamsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListLogStreamsQueryResult>> Handle(
        ListLogStreamsQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.LogGroupName);
        var streams = await _client.ListLogStreamsAsync(request.LogGroupName, cancellationToken);
        LogHandled(streams.IsSuccess);

        if (!streams.IsSuccess)
        {
            Result<ListLogStreamsQueryResult> failure = streams.Error!.Value;
            return failure;
        }

        return new ListLogStreamsQueryResult(streams.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing log streams for CloudWatch log group {LogGroupName}.")]
    private partial void LogHandling(string logGroupName);

    [LoggerMessage(LogLevel.Trace, "CloudWatch log stream listing handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
