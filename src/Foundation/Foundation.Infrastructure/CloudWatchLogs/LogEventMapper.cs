using Amazon.CloudWatchLogs.Model;
using LogEvent = Foundation.Domain.CloudWatchLogs.LogEvent;

namespace Foundation.Infrastructure.CloudWatchLogs;

/// <summary>
/// Translates AWS CloudWatch Logs output event shapes into the domain records the application works
/// with, applying safe defaults for missing values.
/// </summary>
internal static class LogEventMapper
{
    /// <summary>
    /// Map an SDK output log event to the domain log event.
    /// </summary>
    /// <param name="logEvent">The SDK output log event returned by a get-events call.</param>
    /// <returns>The domain log event.</returns>
    public static LogEvent ToLogEvent(OutputLogEvent logEvent)
        => new(
            logEvent.Timestamp is null
                ? DateTimeOffset.UnixEpoch
                : new DateTimeOffset(DateTime.SpecifyKind(logEvent.Timestamp.Value, DateTimeKind.Utc)),
            logEvent.Message ?? string.Empty);

    /// <summary>
    /// Map an SDK filtered log event to the domain log event.
    /// </summary>
    /// <param name="logEvent">The SDK filtered log event returned by a filter-events call.</param>
    /// <returns>The domain log event.</returns>
    public static LogEvent ToFilteredLogEvent(FilteredLogEvent logEvent)
        => new(
            logEvent.Timestamp is null
                ? DateTimeOffset.UnixEpoch
                : DateTimeOffset.FromUnixTimeMilliseconds(logEvent.Timestamp.Value),
            logEvent.Message ?? string.Empty);
}
