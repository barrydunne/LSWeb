using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Scheduler;

namespace Foundation.Application.Queries.ListScheduleGroups;

/// <summary>
/// Lists the EventBridge Scheduler schedule groups available on the backend.
/// </summary>
public record ListScheduleGroupsQuery : IQuery<ListScheduleGroupsQueryResult>;

/// <summary>
/// The result of listing schedule groups.
/// </summary>
/// <param name="Groups">The schedule groups found on the backend.</param>
public record ListScheduleGroupsQueryResult(IReadOnlyList<ScheduleGroup> Groups);
