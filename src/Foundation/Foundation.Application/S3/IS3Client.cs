using AspNet.KickStarter.FunctionalResult;
using Foundation.Domain.S3;

namespace Foundation.Application.S3;

/// <summary>
/// Reads and manages S3 buckets on the configured AWS backend. Implementations route through the
/// resilient AWS gateway and never throw across layers, reporting failures as a
/// <see cref="Result{T}"/>.
/// </summary>
public interface IS3Client
{
    /// <summary>
    /// List the buckets visible to the configured backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The buckets on success; otherwise a failure describing the error.</returns>
    Task<Result<IReadOnlyList<S3Bucket>>> ListBucketsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Create a new bucket with the supplied name.
    /// </summary>
    /// <param name="bucketName">The name of the bucket to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A successful result on completion; otherwise a failure describing the error.</returns>
    Task<Result> CreateBucketAsync(string bucketName, CancellationToken cancellationToken);

    /// <summary>
    /// Delete a bucket with the supplied name.
    /// </summary>
    /// <param name="bucketName">The name of the bucket to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A successful result on completion; otherwise a failure describing the error.</returns>
    Task<Result> DeleteBucketAsync(string bucketName, CancellationToken cancellationToken);
}
