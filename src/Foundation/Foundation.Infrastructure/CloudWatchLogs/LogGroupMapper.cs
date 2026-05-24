using LogGroup = Foundation.Domain.CloudWatchLogs.LogGroup;

namespace Foundation.Infrastructure.CloudWatchLogs;

/// <summary>
/// Translates AWS CloudWatch Logs log group shapes into the domain records the application works
/// with, applying safe defaults for missing values.
/// </summary>
internal static class LogGroupMapper
{
    /// <summary>
    /// Map an SDK log group to the domain log group.
    /// </summary>
    /// <param name="logGroup">The SDK log group returned by a describe call.</param>
    /// <returns>The domain log group.</returns>
    public static LogGroup ToLogGroup(Amazon.CloudWatchLogs.Model.LogGroup logGroup)
        => new(
            logGroup.LogGroupName ?? string.Empty,
            logGroup.Arn ?? string.Empty,
            logGroup.StoredBytes ?? 0,
            logGroup.RetentionInDays,
            logGroup.CreationTime is null
                ? null
                : new DateTimeOffset(DateTime.SpecifyKind(logGroup.CreationTime.Value, DateTimeKind.Utc)));
}
