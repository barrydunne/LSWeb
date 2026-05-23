namespace Foundation.Domain.S3;

/// <summary>
/// A best-effort storage summary for a single S3 bucket, aggregated from the objects it contains.
/// Zero-byte folder marker keys are excluded so the counts reflect the real files stored.
/// </summary>
/// <param name="ObjectCount">The number of objects stored in the bucket.</param>
/// <param name="TotalSizeBytes">The total size of the stored objects in bytes.</param>
public sealed record S3BucketStorageSummary(
    long ObjectCount,
    long TotalSizeBytes);
