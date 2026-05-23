using AspNet.KickStarter.CQRS.Abstractions.Queries;

namespace Foundation.Application.Queries.GetS3BucketStorageSummary;

/// <summary>
/// Aggregate a best-effort storage summary for a single S3 bucket: the number of objects and their
/// total size in bytes.
/// </summary>
/// <param name="BucketName">The bucket to summarize.</param>
public record GetS3BucketStorageSummaryQuery(string BucketName) : IQuery<GetS3BucketStorageSummaryQueryResult>;

/// <summary>
/// The aggregated storage summary of a single S3 bucket.
/// </summary>
/// <param name="ObjectCount">The number of objects stored in the bucket.</param>
/// <param name="TotalSizeBytes">The total size of the stored objects in bytes.</param>
public record GetS3BucketStorageSummaryQueryResult(
    long ObjectCount,
    long TotalSizeBytes);
