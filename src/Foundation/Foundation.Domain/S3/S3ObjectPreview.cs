namespace Foundation.Domain.S3;

/// <summary>
/// A size-limited preview of a single S3 object, materialized for inline display without
/// downloading the entire object.
/// </summary>
/// <param name="Content">The leading bytes of the object, capped at the requested preview size.</param>
/// <param name="ContentType">The reported content type; falls back to a binary default when unset.</param>
/// <param name="TotalSize">The full size of the object in bytes.</param>
/// <param name="Truncated">Whether the object is larger than the materialized preview content.</param>
public sealed record S3ObjectPreview(byte[] Content, string ContentType, long TotalSize, bool Truncated);
