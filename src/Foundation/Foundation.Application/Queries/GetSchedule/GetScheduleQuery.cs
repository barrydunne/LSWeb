using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Scheduler;

namespace Foundation.Application.Queries.GetSchedule;

/// <summary>
/// Reads the full configuration of a single EventBridge Scheduler schedule.
/// </summary>
/// <param name="Name">The name of the schedule.</param>
/// <param name="GroupName">The name of the schedule group the schedule belongs to.</param>
public record GetScheduleQuery(string Name, string GroupName) : IQuery<GetScheduleQueryResult>;

/// <summary>
/// The result of reading a schedule.
/// </summary>
/// <param name="Schedule">The schedule detail.</param>
public record GetScheduleQueryResult(ScheduleDetail Schedule);
