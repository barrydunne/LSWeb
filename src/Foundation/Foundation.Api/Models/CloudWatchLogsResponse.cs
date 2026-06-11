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

/// <summary>
/// A request to create a new CloudWatch log stream within a log group.
/// </summary>
/// <param name="LogGroupName">The name of the log group the stream belongs to.</param>
/// <param name="LogStreamName">The name of the log stream to create.</param>
public sealed record LogStreamCreateRequest(string LogGroupName, string LogStreamName);

/// <summary>
/// A request to run a CloudWatch Logs Insights query against a log group over a time range.
/// </summary>
/// <param name="LogGroupName">The name of the log group to query.</param>
/// <param name="QueryString">The CloudWatch Logs Insights query to run.</param>
/// <param name="StartTime">The inclusive lower bound of the query time range.</param>
/// <param name="EndTime">The inclusive upper bound of the query time range.</param>
/// <param name="Limit">The maximum number of result rows to return.</param>
public sealed record LogInsightsQueryRequest(
    string LogGroupName,
    string QueryString,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    int Limit);

/// <summary>
/// The outcome of a CloudWatch Logs Insights query.
/// </summary>
/// <param name="Status">The terminal query status reported by the backend.</param>
/// <param name="Rows">The matching result rows in the order returned by the backend.</param>
/// <param name="RecordsMatched">The number of log records that matched the query.</param>
/// <param name="RecordsScanned">The number of log records the backend scanned to run the query.</param>
public sealed record LogInsightsQueryResponse(
    string Status,
    IReadOnlyList<LogInsightsRowResponse> Rows,
    long RecordsMatched,
    long RecordsScanned);

/// <summary>
/// A single row returned by a CloudWatch Logs Insights query.
/// </summary>
/// <param name="Fields">The fields that make up the row, in the order returned by the backend.</param>
public sealed record LogInsightsRowResponse(IReadOnlyList<LogInsightsFieldResponse> Fields);

/// <summary>
/// A single field within a CloudWatch Logs Insights result row.
/// </summary>
/// <param name="Field">The name of the field.</param>
/// <param name="Value">The value of the field for the owning row.</param>
public sealed record LogInsightsFieldResponse(string Field, string Value);
