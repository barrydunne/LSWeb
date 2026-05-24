using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.CloudWatchLogs;

namespace Foundation.Application.Queries.ListLogGroups;

/// <summary>
/// List the CloudWatch log groups available on the configured backend.
/// </summary>
public record ListLogGroupsQuery : IQuery<ListLogGroupsQueryResult>;

/// <summary>
/// The CloudWatch log groups returned by the backend.
/// </summary>
/// <param name="LogGroups">The log groups, ordered as returned by the backend.</param>
public record ListLogGroupsQueryResult(IReadOnlyList<LogGroup> LogGroups);
