namespace Foundation.Domain.S3;

/// <summary>
/// A single object stored within an S3 bucket, as seen when browsing a prefix.
/// </summary>
/// <param name="Key">The full key of the object within the bucket.</param>
/// <param name="Size">The size of the object in bytes.</param>
/// <param name="LastModified">The timestamp the object was last modified; empty when not reported.</param>
public sealed record S3Object(string Key, long Size, string LastModified);

/// <summary>
/// The contents of a single bucket prefix when navigated as a folder: the immediate child prefixes
/// (folders) and the objects (files) that live directly under the prefix.
/// </summary>
/// <param name="Prefixes">The immediate child prefixes, each ending with a delimiter.</param>
/// <param name="Objects">The objects that live directly under the requested prefix.</param>
public sealed record S3ObjectListing(
    IReadOnlyList<string> Prefixes,
    IReadOnlyList<S3Object> Objects);
