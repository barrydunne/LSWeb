using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Scheduler;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetSchedule;

internal sealed partial class GetScheduleQueryHandler : IQueryHandler<GetScheduleQuery, GetScheduleQueryResult>
{
    private readonly ISchedulerClient _client;
    private readonly ILogger _logger;

    public GetScheduleQueryHandler(ISchedulerClient client, ILogger<GetScheduleQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetScheduleQueryResult>> Handle(GetScheduleQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.GroupName, request.Name);
        var schedule = await _client.GetScheduleAsync(request.Name, request.GroupName, cancellationToken);
        LogHandled(schedule.IsSuccess);

        if (!schedule.IsSuccess)
        {
            Result<GetScheduleQueryResult> failure = schedule.Error!.Value;
            return failure;
        }

        return new GetScheduleQueryResult(schedule.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Reading Scheduler schedule {GroupName}/{Name}.")]
    private partial void LogHandling(string groupName, string name);

    [LoggerMessage(LogLevel.Trace, "Scheduler schedule read handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
