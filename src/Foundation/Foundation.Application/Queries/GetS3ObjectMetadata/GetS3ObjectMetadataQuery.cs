using AspNet.KickStarter.CQRS.Abstractions.Queries;

namespace Foundation.Application.Queries.GetS3ObjectMetadata;

/// <summary>
/// Read the system properties, user metadata and tags recorded against a single S3 object.
/// </summary>
/// <param name="BucketName">The bucket the object lives in.</param>
/// <param name="Key">The full key of the object within the bucket.</param>
public record GetS3ObjectMetadataQuery(string BucketName, string Key) : IQuery<GetS3ObjectMetadataQueryResult>;

/// <summary>
/// The system properties, user metadata and tags recorded against a single S3 object.
/// </summary>
/// <param name="ContentType">The reported content type of the object.</param>
/// <param name="ContentLength">The size of the object in bytes.</param>
/// <param name="LastModified">The timestamp the object was last modified; empty when not reported.</param>
/// <param name="ETag">The entity tag identifying the current object content; empty when not reported.</param>
/// <param name="Metadata">The user-defined metadata entries ordered by name.</param>
/// <param name="Tags">The object tag entries ordered by name.</param>
public record GetS3ObjectMetadataQueryResult(
    string ContentType,
    long ContentLength,
    string LastModified,
    string ETag,
    IReadOnlyList<S3MetadataEntry> Metadata,
    IReadOnlyList<S3MetadataEntry> Tags);

/// <summary>
/// A single name/value entry of user metadata or an object tag.
/// </summary>
/// <param name="Key">The entry name.</param>
/// <param name="Value">The entry value.</param>
public record S3MetadataEntry(string Key, string Value);
