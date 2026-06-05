using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Scheduler;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListScheduleGroups;

internal sealed partial class ListScheduleGroupsQueryHandler : IQueryHandler<ListScheduleGroupsQuery, ListScheduleGroupsQueryResult>
{
    private readonly ISchedulerClient _client;
    private readonly ILogger _logger;

    public ListScheduleGroupsQueryHandler(ISchedulerClient client, ILogger<ListScheduleGroupsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListScheduleGroupsQueryResult>> Handle(ListScheduleGroupsQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var groups = await _client.ListScheduleGroupsAsync(cancellationToken);
        LogHandled(groups.IsSuccess);

        if (!groups.IsSuccess)
        {
            Result<ListScheduleGroupsQueryResult> failure = groups.Error!.Value;
            return failure;
        }

        return new ListScheduleGroupsQueryResult(groups.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing Scheduler schedule groups.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "Scheduler schedule group listing handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
