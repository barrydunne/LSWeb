using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.CloudWatchLogs;

namespace Foundation.Application.Queries.FilterLogEvents;

/// <summary>
/// Search the events across every stream in a CloudWatch log group, optionally constrained by a
/// filter pattern and a start time. Powers filtered search and live tail.
/// </summary>
/// <param name="LogGroupName">The name of the log group to search.</param>
/// <param name="FilterPattern">The CloudWatch Logs filter pattern, or <see langword="null"/> for no filter.</param>
/// <param name="StartTime">Only return events at or after this time, or <see langword="null"/> for no lower bound.</param>
/// <param name="Limit">The maximum number of events to return.</param>
public record FilterLogEventsQuery(string LogGroupName, string? FilterPattern, DateTimeOffset? StartTime, int Limit)
    : IQuery<FilterLogEventsQueryResult>;

/// <summary>
/// The CloudWatch log events returned by the backend.
/// </summary>
/// <param name="Events">The matching log events in chronological order.</param>
public record FilterLogEventsQueryResult(IReadOnlyList<LogEvent> Events);
