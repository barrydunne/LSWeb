using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.CloudWatchLogs;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListLogGroups;

internal sealed partial class ListLogGroupsQueryHandler : IQueryHandler<ListLogGroupsQuery, ListLogGroupsQueryResult>
{
    private readonly ICloudWatchLogsClient _client;
    private readonly ILogger _logger;

    public ListLogGroupsQueryHandler(ICloudWatchLogsClient client, ILogger<ListLogGroupsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListLogGroupsQueryResult>> Handle(ListLogGroupsQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var groups = await _client.ListLogGroupsAsync(cancellationToken);
        LogHandled(groups.IsSuccess);

        if (!groups.IsSuccess)
        {
            Result<ListLogGroupsQueryResult> failure = groups.Error!.Value;
            return failure;
        }

        return new ListLogGroupsQueryResult(groups.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing CloudWatch log groups.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "CloudWatch log group listing handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
