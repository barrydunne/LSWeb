namespace Foundation.Api.Models;

/// <summary>
/// The CloudWatch log groups available on the configured backend.
/// </summary>
/// <param name="LogGroups">The log group summaries, ordered as returned by the backend.</param>
public sealed record LogGroupListResponse(IReadOnlyList<LogGroupResponse> LogGroups);

/// <summary>
/// A concise view of a CloudWatch log group as it appears in a log group list.
/// </summary>
/// <param name="Name">The name of the log group.</param>
/// <param name="Arn">The fully-qualified ARN of the log group.</param>
/// <param name="StoredBytes">The approximate number of bytes stored in the log group.</param>
/// <param name="RetentionInDays">The retention period in days, or <see langword="null"/> if logs never expire.</param>
/// <param name="CreatedAt">The time the log group was created, if reported by the backend.</param>
public sealed record LogGroupResponse(
    string Name,
    string Arn,
    long StoredBytes,
    int? RetentionInDays,
    DateTimeOffset? CreatedAt);

/// <summary>
/// The CloudWatch log streams within a log group.
/// </summary>
/// <param name="LogStreams">The log stream summaries, most recently active first.</param>
public sealed record LogStreamListResponse(IReadOnlyList<LogStreamResponse> LogStreams);

/// <summary>
/// A concise view of a CloudWatch log stream within a log group.
/// </summary>
/// <param name="Name">The name of the log stream.</param>
/// <param name="LastEventTimestamp">The time of the most recent event in the stream, if reported by the backend.</param>
public sealed record LogStreamResponse(
    string Name,
    DateTimeOffset? LastEventTimestamp);

/// <summary>
/// The CloudWatch log events read from a log stream.
/// </summary>
/// <param name="Events">The log events in chronological order.</param>
public sealed record LogEventListResponse(IReadOnlyList<LogEventResponse> Events);

/// <summary>
/// A single CloudWatch log event.
/// </summary>
/// <param name="Timestamp">The time the event was recorded.</param>
/// <param name="Message">The raw event message as stored by the backend.</param>
public sealed record LogEventResponse(
    DateTimeOffset Timestamp,
    string Message);

/// <summary>
/// A request to create a new CloudWatch log group.
/// </summary>
/// <param name="LogGroupName">The name of the log group to create.</param>
public sealed record LogGroupCreateRequest(string LogGroupName);
