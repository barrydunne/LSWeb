namespace Foundation.Domain.S3;

/// <summary>
/// A single version of an S3 object within a versioning-enabled bucket.
/// </summary>
/// <param name="Key">The object key.</param>
/// <param name="VersionId">The version identifier; <c>null</c> sentinel value <c>"null"</c> for pre-versioning objects.</param>
/// <param name="IsLatest">Whether this is the current version of the object.</param>
/// <param name="IsDeleteMarker">Whether this version is a delete marker rather than stored content.</param>
/// <param name="Size">The size of the version in bytes; zero for delete markers.</param>
/// <param name="LastModified">The timestamp the version was created, as reported by AWS.</param>
public sealed record S3ObjectVersion(
    string Key,
    string VersionId,
    bool IsLatest,
    bool IsDeleteMarker,
    long Size,
    string LastModified);
