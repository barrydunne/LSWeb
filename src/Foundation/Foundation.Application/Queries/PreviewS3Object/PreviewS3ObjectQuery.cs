using AspNet.KickStarter.CQRS.Abstractions.Queries;

namespace Foundation.Application.Queries.PreviewS3Object;

/// <summary>
/// Read a size-limited, classified preview of a single object from an S3 bucket.
/// </summary>
/// <param name="BucketName">The bucket the object lives in.</param>
/// <param name="Key">The full key of the object within the bucket.</param>
public record PreviewS3ObjectQuery(string BucketName, string Key) : IQuery<PreviewS3ObjectQueryResult>;

/// <summary>
/// A classified preview of a single S3 object.
/// </summary>
/// <param name="Kind">How the preview should be rendered (Text, Json, Image, or Binary).</param>
/// <param name="ContentType">The reported content type of the object.</param>
/// <param name="Truncated">Whether the object is larger than the materialized preview content.</param>
/// <param name="TotalSize">The full size of the object in bytes.</param>
/// <param name="Text">The decoded text for text or JSON previews; otherwise null.</param>
/// <param name="DataUrl">A data URL for image previews; otherwise null.</param>
public record PreviewS3ObjectQueryResult(
    string Kind,
    string ContentType,
    bool Truncated,
    long TotalSize,
    string? Text,
    string? DataUrl);
