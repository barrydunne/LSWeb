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

/// <summary>
/// The folders and objects directly beneath a prefix within a bucket.
/// </summary>
/// <param name="Prefixes">The immediate child prefixes (folders), each ending with a delimiter.</param>
/// <param name="Objects">The objects (files) that live directly under the requested prefix.</param>
public sealed record S3ObjectListingResponse(
    IReadOnlyList<string> Prefixes,
    IReadOnlyList<S3ObjectResponse> Objects);

/// <summary>
/// A concise view of an S3 object as it appears when browsing a prefix.
/// </summary>
/// <param name="Key">The full key of the object within the bucket.</param>
/// <param name="Size">The size of the object in bytes.</param>
/// <param name="LastModified">The timestamp the object was last modified; empty when not reported.</param>
public sealed record S3ObjectResponse(
    string Key,
    long Size,
    string LastModified);

/// <summary>
/// A request to create a zero-byte folder marker within a bucket.
/// </summary>
/// <param name="FolderKey">The full folder key, ending with a delimiter.</param>
public sealed record S3FolderCreateRequest(string FolderKey);

/// <summary>
/// A multipart request to upload an object into a bucket beneath an optional prefix.
/// </summary>
public sealed class S3ObjectUploadRequest
{
    /// <summary>
    /// Gets the file to upload.
    /// </summary>
    public IFormFile File { get; init; } = default!;

    /// <summary>
    /// Gets the prefix the object is uploaded beneath; empty uploads to the bucket root.
    /// </summary>
    public string? Prefix { get; init; }
}

/// <summary>
/// A classified, size-limited preview of a single S3 object for inline display.
/// </summary>
/// <param name="Kind">How the preview should be rendered: Text, Json, Image, or Binary.</param>
/// <param name="ContentType">The reported content type of the object.</param>
/// <param name="Truncated">Whether the object is larger than the materialized preview content.</param>
/// <param name="TotalSize">The full size of the object in bytes.</param>
/// <param name="Text">The decoded text for text or JSON previews; otherwise null.</param>
/// <param name="DataUrl">A data URL for image previews; otherwise null.</param>
public sealed record S3ObjectPreviewResponse(
    string Kind,
    string ContentType,
    bool Truncated,
    long TotalSize,
    string? Text,
    string? DataUrl);

/// <summary>
/// A generated presigned URL for a single S3 object and the effective expiry applied.
/// </summary>
/// <param name="Url">The presigned URL clients can use to fetch the object directly.</param>
/// <param name="ExpirySeconds">The effective lifetime of the URL in seconds after clamping.</param>
public sealed record S3PresignedUrlResponse(string Url, int ExpirySeconds);

/// <summary>
/// The system properties, user metadata and tags recorded against a single S3 object.
/// </summary>
/// <param name="ContentType">The reported content type of the object.</param>
/// <param name="ContentLength">The size of the object in bytes.</param>
/// <param name="LastModified">The timestamp the object was last modified; empty when not reported.</param>
/// <param name="ETag">The entity tag identifying the current object content; empty when not reported.</param>
/// <param name="Metadata">The user-defined metadata entries ordered by name.</param>
/// <param name="Tags">The object tag entries ordered by name.</param>
public sealed record S3ObjectMetadataResponse(
    string ContentType,
    long ContentLength,
    string LastModified,
    string ETag,
    IReadOnlyList<S3MetadataEntryResponse> Metadata,
    IReadOnlyList<S3MetadataEntryResponse> Tags);

/// <summary>
/// A single name/value entry of user metadata or an object tag.
/// </summary>
/// <param name="Key">The entry name.</param>
/// <param name="Value">The entry value.</param>
public sealed record S3MetadataEntryResponse(string Key, string Value);

/// <summary>
/// A request to replace the full set of tags recorded against a single S3 object.
/// </summary>
/// <param name="Tags">The full set of tags to apply, keyed by tag name.</param>
public sealed record S3ObjectTagsUpdateRequest(IReadOnlyDictionary<string, string> Tags);

/// <summary>
/// A request to copy or move a single S3 object to a destination key, optionally in another bucket.
/// </summary>
/// <param name="DestinationBucketName">The bucket the object is copied or moved into.</param>
/// <param name="DestinationKey">The full key of the destination object.</param>
public sealed record S3ObjectCopyRequest(string DestinationBucketName, string DestinationKey);

/// <summary>
/// The resolved configuration of a single S3 bucket.
/// </summary>
/// <param name="VersioningStatus">The bucket versioning status: <c>Enabled</c>, <c>Suspended</c> or <c>Disabled</c>.</param>
/// <param name="EncryptionAlgorithm">The default server-side encryption algorithm; empty when none is configured.</param>
/// <param name="EncryptionKeyId">The KMS key id used by default encryption; empty when not applicable.</param>
/// <param name="LifecycleRules">The lifecycle rules defined on the bucket, ordered by id.</param>
/// <param name="Notifications">The event notification configurations defined on the bucket.</param>
/// <param name="Policy">The bucket access policy document as raw JSON; empty when none is configured.</param>
public sealed record S3BucketConfigurationResponse(
    string VersioningStatus,
    string EncryptionAlgorithm,
    string EncryptionKeyId,
    IReadOnlyList<S3LifecycleRuleResponse> LifecycleRules,
    IReadOnlyList<S3NotificationResponse> Notifications,
    string Policy);

/// <summary>
/// A summary of a single S3 lifecycle rule.
/// </summary>
/// <param name="Id">The rule identifier; empty when the bucket did not name the rule.</param>
/// <param name="Status">The rule status: <c>Enabled</c> or <c>Disabled</c>.</param>
/// <param name="Prefix">The object key prefix the rule applies to; empty when the rule applies to the whole bucket.</param>
public sealed record S3LifecycleRuleResponse(string Id, string Status, string Prefix);

/// <summary>
/// A single S3 event notification configuration and the cross-resource target it delivers to.
/// </summary>
/// <param name="Type">The target resource type: <c>Lambda</c>, <c>Queue</c> or <c>Topic</c>.</param>
/// <param name="TargetArn">The ARN of the target Lambda function, SQS queue or SNS topic.</param>
/// <param name="Events">The S3 event names that trigger the notification.</param>
public sealed record S3NotificationResponse(string Type, string TargetArn, IReadOnlyList<string> Events);

/// <summary>
/// A best-effort storage summary for a single S3 bucket.
/// </summary>
/// <param name="ObjectCount">The number of objects stored in the bucket.</param>
/// <param name="TotalSizeBytes">The total size of the stored objects in bytes.</param>
public sealed record S3BucketStorageSummaryResponse(
    long ObjectCount,
    long TotalSizeBytes);

