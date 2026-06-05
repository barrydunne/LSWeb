using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Scheduler;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListSchedules;

internal sealed partial class ListSchedulesQueryHandler : IQueryHandler<ListSchedulesQuery, ListSchedulesQueryResult>
{
    private readonly ISchedulerClient _client;
    private readonly ILogger _logger;

    public ListSchedulesQueryHandler(ISchedulerClient client, ILogger<ListSchedulesQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListSchedulesQueryResult>> Handle(ListSchedulesQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var schedules = await _client.ListSchedulesAsync(cancellationToken);
        LogHandled(schedules.IsSuccess);

        if (!schedules.IsSuccess)
        {
            Result<ListSchedulesQueryResult> failure = schedules.Error!.Value;
            return failure;
        }

        return new ListSchedulesQueryResult(schedules.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing Scheduler schedules.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "Scheduler schedule listing handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
