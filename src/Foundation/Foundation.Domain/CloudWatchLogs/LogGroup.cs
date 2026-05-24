namespace Foundation.Domain.CloudWatchLogs;

/// <summary>
/// A concise view of a CloudWatch log group as it appears in a log group list.
/// </summary>
/// <param name="Name">The name of the log group.</param>
/// <param name="Arn">The fully-qualified ARN of the log group.</param>
/// <param name="StoredBytes">The approximate number of bytes stored in the log group.</param>
/// <param name="RetentionInDays">The retention period in days, or <see langword="null"/> if logs never expire.</param>
/// <param name="CreatedAt">The time the log group was created, if reported by the backend.</param>
public sealed record LogGroup(
    string Name,
    string Arn,
    long StoredBytes,
    int? RetentionInDays,
    DateTimeOffset? CreatedAt);
