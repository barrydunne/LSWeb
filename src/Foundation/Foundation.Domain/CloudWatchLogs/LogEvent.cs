namespace Foundation.Domain.CloudWatchLogs;

/// <summary>
/// A single log event read from a CloudWatch log stream.
/// </summary>
/// <param name="Timestamp">The time the event was recorded.</param>
/// <param name="Message">The raw event message as stored by the backend.</param>
public sealed record LogEvent(
    DateTimeOffset Timestamp,
    string Message);
