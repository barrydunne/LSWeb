namespace Foundation.Api.Models;

/// <summary>
/// The S3 buckets available on the configured backend.
/// </summary>
/// <param name="Buckets">The bucket summaries, ordered as returned by the backend.</param>
public sealed record S3BucketListResponse(IReadOnlyList<S3BucketResponse> Buckets);

/// <summary>
/// A concise view of an S3 bucket as it appears in a bucket list.
/// </summary>
/// <param name="Name">The globally unique name of the bucket.</param>
/// <param name="CreationDate">The timestamp the bucket was created; empty when not reported.</param>
public sealed record S3BucketResponse(
    string Name,
    string CreationDate);

/// <summary>
/// A request to create a new S3 bucket.
/// </summary>
/// <param name="BucketName">The name of the bucket to create.</param>
public sealed record S3BucketCreateRequest(string BucketName);
