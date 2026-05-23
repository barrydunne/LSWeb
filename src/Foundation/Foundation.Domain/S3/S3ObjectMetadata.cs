namespace Foundation.Domain.S3;

/// <summary>
/// The system properties, user metadata and tags recorded against a single S3 object.
/// </summary>
/// <param name="ContentType">The reported content type of the object.</param>
/// <param name="ContentLength">The size of the object in bytes.</param>
/// <param name="LastModified">The timestamp the object was last modified; empty when not reported.</param>
/// <param name="ETag">The entity tag identifying the current object content; empty when not reported.</param>
/// <param name="UserMetadata">The user-defined metadata keyed by name, with the x-amz-meta- prefix stripped.</param>
/// <param name="Tags">The object tags keyed by tag name.</param>
public record S3ObjectMetadata(
    string ContentType,
    long ContentLength,
    string LastModified,
    string ETag,
    IReadOnlyDictionary<string, string> UserMetadata,
    IReadOnlyDictionary<string, string> Tags);
