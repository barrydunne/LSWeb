using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Scheduler;

namespace Foundation.Application.Queries.ListSchedules;

/// <summary>
/// Lists the EventBridge Scheduler schedules across all schedule groups.
/// </summary>
public record ListSchedulesQuery : IQuery<ListSchedulesQueryResult>;

/// <summary>
/// The result of listing schedules.
/// </summary>
/// <param name="Schedules">The schedules found on the backend.</param>
public record ListSchedulesQueryResult(IReadOnlyList<ScheduleSummary> Schedules);
