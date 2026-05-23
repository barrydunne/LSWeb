namespace Foundation.Domain.S3;

/// <summary>
/// The materialized content of a single S3 object retrieved for download.
/// </summary>
/// <param name="Content">The object's bytes.</param>
/// <param name="ContentType">The reported content type; falls back to a binary default when unset.</param>
public sealed record S3ObjectContent(byte[] Content, string ContentType);
