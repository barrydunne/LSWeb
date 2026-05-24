using LogStream = Foundation.Domain.CloudWatchLogs.LogStream;

namespace Foundation.Infrastructure.CloudWatchLogs;

/// <summary>
/// Translates AWS CloudWatch Logs log stream shapes into the domain records the application works
/// with, applying safe defaults for missing values.
/// </summary>
internal static class LogStreamMapper
{
    /// <summary>
    /// Map an SDK log stream to the domain log stream.
    /// </summary>
    /// <param name="logStream">The SDK log stream returned by a describe call.</param>
    /// <returns>The domain log stream.</returns>
    public static LogStream ToLogStream(Amazon.CloudWatchLogs.Model.LogStream logStream)
        => new(
            logStream.LogStreamName ?? string.Empty,
            ToTimestamp(logStream.LastEventTimestamp));

    private static DateTimeOffset? ToTimestamp(DateTime? value)
        => value is null
            ? null
            : new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc));
}
