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

    /// <summary>
    /// List the immediate child prefixes (folders) and objects (files) directly beneath a prefix.
    /// </summary>
    /// <param name="bucketName">The bucket to browse.</param>
    /// <param name="prefix">The prefix to list beneath; empty lists the bucket root.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The folders and objects on success; otherwise a failure describing the error.</returns>
    Task<Result<S3ObjectListing>> ListObjectsAsync(string bucketName, string prefix, CancellationToken cancellationToken);

    /// <summary>
    /// Create a zero-byte folder marker object so an empty prefix appears as a navigable folder.
    /// </summary>
    /// <param name="bucketName">The bucket the folder lives in.</param>
    /// <param name="folderKey">The full folder key, ending with a delimiter.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A successful result on completion; otherwise a failure describing the error.</returns>
    Task<Result> CreateFolderAsync(string bucketName, string folderKey, CancellationToken cancellationToken);

    /// <summary>
    /// Upload an object to a bucket, streaming the supplied content under the given key.
    /// </summary>
    /// <param name="bucketName">The bucket the object lives in.</param>
    /// <param name="key">The full key of the object within the bucket.</param>
    /// <param name="content">The content stream to upload.</param>
    /// <param name="contentType">The content type to record for the object.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A successful result on completion; otherwise a failure describing the error.</returns>
    Task<Result> UploadObjectAsync(string bucketName, string key, Stream content, string contentType, CancellationToken cancellationToken);

    /// <summary>
    /// Download the content of a single object from a bucket.
    /// </summary>
    /// <param name="bucketName">The bucket the object lives in.</param>
    /// <param name="key">The full key of the object within the bucket.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The object content on success; otherwise a failure describing the error.</returns>
    Task<Result<S3ObjectContent>> DownloadObjectAsync(string bucketName, string key, CancellationToken cancellationToken);

    /// <summary>
    /// Read a size-limited preview of a single object so it can be displayed inline without
    /// downloading the entire object.
    /// </summary>
    /// <param name="bucketName">The bucket the object lives in.</param>
    /// <param name="key">The full key of the object within the bucket.</param>
    /// <param name="maxBytes">The maximum number of leading bytes to materialize for the preview.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The object preview on success; otherwise a failure describing the error.</returns>
    Task<Result<S3ObjectPreview>> PreviewObjectAsync(string bucketName, string key, int maxBytes, CancellationToken cancellationToken);

    /// <summary>
    /// Generate a time-limited presigned GET URL for a single object.
    /// </summary>
    /// <param name="bucketName">The bucket the object lives in.</param>
    /// <param name="key">The full key of the object within the bucket.</param>
    /// <param name="expiresIn">How long the generated URL remains valid.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The presigned URL on success; otherwise a failure describing the error.</returns>
    Task<Result<string>> GeneratePresignedUrlAsync(string bucketName, string key, TimeSpan expiresIn, CancellationToken cancellationToken);

    /// <summary>
    /// Read the system properties, user metadata and tags recorded against a single object.
    /// </summary>
    /// <param name="bucketName">The bucket the object lives in.</param>
    /// <param name="key">The full key of the object within the bucket.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The object metadata on success; otherwise a failure describing the error.</returns>
    Task<Result<S3ObjectMetadata>> GetObjectMetadataAsync(string bucketName, string key, CancellationToken cancellationToken);

    /// <summary>
    /// Replace the full set of tags recorded against a single object.
    /// </summary>
    /// <param name="bucketName">The bucket the object lives in.</param>
    /// <param name="key">The full key of the object within the bucket.</param>
    /// <param name="tags">The full set of tags to apply, keyed by tag name.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A successful result on completion; otherwise a failure describing the error.</returns>
    Task<Result> UpdateObjectTagsAsync(
        string bucketName, string key, IReadOnlyDictionary<string, string> tags, CancellationToken cancellationToken);

    /// <summary>
    /// Delete a single object from a bucket.
    /// </summary>
    /// <param name="bucketName">The bucket the object lives in.</param>
    /// <param name="key">The full key of the object within the bucket.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A successful result on completion; otherwise a failure describing the error.</returns>
    Task<Result> DeleteObjectAsync(string bucketName, string key, CancellationToken cancellationToken);

    /// <summary>
    /// Copy a single object to a destination key, optionally in a different bucket.
    /// </summary>
    /// <param name="sourceBucketName">The bucket the source object lives in.</param>
    /// <param name="sourceKey">The full key of the source object.</param>
    /// <param name="destinationBucketName">The bucket the object is copied into.</param>
    /// <param name="destinationKey">The full key of the copied object.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A successful result on completion; otherwise a failure describing the error.</returns>
    Task<Result> CopyObjectAsync(
        string sourceBucketName,
        string sourceKey,
        string destinationBucketName,
        string destinationKey,
        CancellationToken cancellationToken);

    /// <summary>
    /// Read the configuration of a single bucket: versioning, default encryption, lifecycle rules,
    /// event notifications and the access policy. Aspects that are not configured are returned as
    /// their empty value rather than a failure so the whole view always renders.
    /// </summary>
    /// <param name="bucketName">The bucket to read the configuration of.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The bucket configuration on success; otherwise a failure describing the error.</returns>
    Task<Result<S3BucketConfiguration>> GetBucketConfigurationAsync(string bucketName, CancellationToken cancellationToken);

    /// <summary>
    /// Aggregate a best-effort storage summary for a single bucket: the number of objects and their
    /// total size in bytes, derived by paging through the objects the bucket contains. Zero-byte
    /// folder marker keys are excluded so the summary reflects the real files stored.
    /// </summary>
    /// <param name="bucketName">The bucket to summarize.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The storage summary on success; otherwise a failure describing the error.</returns>
    Task<Result<S3BucketStorageSummary>> GetBucketStorageSummaryAsync(string bucketName, CancellationToken cancellationToken);

    /// <summary>
    /// Apply an access policy to a bucket, replacing any existing policy.
    /// </summary>
    /// <param name="bucketName">The bucket to apply the policy to.</param>
    /// <param name="policy">The policy document as JSON.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or a failure describing the error.</returns>
    Task<Result> PutBucketPolicyAsync(string bucketName, string policy, CancellationToken cancellationToken);

    /// <summary>
    /// Remove the access policy from a bucket.
    /// </summary>
    /// <param name="bucketName">The bucket to remove the policy from.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or a failure describing the error.</returns>
    Task<Result> DeleteBucketPolicyAsync(string bucketName, CancellationToken cancellationToken);

    /// <summary>
    /// Enable or suspend versioning on a bucket.
    /// </summary>
    /// <param name="bucketName">The bucket to update.</param>
    /// <param name="enabled">When <see langword="true"/> versioning is enabled; otherwise it is suspended.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or a failure describing the error.</returns>
    Task<Result> SetBucketVersioningAsync(string bucketName, bool enabled, CancellationToken cancellationToken);

    /// <summary>
    /// List the object versions in a bucket, optionally filtered by key prefix.
    /// </summary>
    /// <param name="bucketName">The bucket to list versions in.</param>
    /// <param name="prefix">The key prefix to filter by; empty for the whole bucket.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The object versions, or a failure describing the error.</returns>
    Task<Result<IReadOnlyList<S3ObjectVersion>>> ListObjectVersionsAsync(
        string bucketName, string prefix, CancellationToken cancellationToken);

    /// <summary>
    /// Delete a specific version of an object.
    /// </summary>
    /// <param name="bucketName">The bucket containing the object.</param>
    /// <param name="key">The object key.</param>
    /// <param name="versionId">The version identifier to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or a failure describing the error.</returns>
    Task<Result> DeleteObjectVersionAsync(
        string bucketName, string key, string versionId, CancellationToken cancellationToken);

    /// <summary>
    /// Replace the event notification configuration of a bucket with the supplied set of rules.
    /// </summary>
    /// <param name="bucketName">The bucket to update.</param>
    /// <param name="notifications">The complete set of notification rules to apply; an empty list clears all notifications.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or a failure describing the error.</returns>
    Task<Result> PutBucketNotificationsAsync(
        string bucketName, IReadOnlyList<S3NotificationConfiguration> notifications, CancellationToken cancellationToken);
}
