using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.CloudWatchLogs;

namespace Foundation.Application.Queries.GetLogEvents;

/// <summary>
/// Read the most recent events from a CloudWatch log stream.
/// </summary>
/// <param name="LogGroupName">The name of the log group the stream belongs to.</param>
/// <param name="LogStreamName">The name of the log stream to read from.</param>
/// <param name="Limit">The maximum number of events to return.</param>
public record GetLogEventsQuery(string LogGroupName, string LogStreamName, int Limit)
    : IQuery<GetLogEventsQueryResult>;

/// <summary>
/// The CloudWatch log events returned by the backend.
/// </summary>
/// <param name="Events">The log events in chronological order.</param>
public record GetLogEventsQueryResult(IReadOnlyList<LogEvent> Events);
