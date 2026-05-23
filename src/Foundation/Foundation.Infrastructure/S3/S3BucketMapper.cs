using System.Globalization;
using Foundation.Domain.S3;

namespace Foundation.Infrastructure.S3;

/// <summary>
/// Translates AWS S3 SDK models into the domain records the application works with, applying safe
/// defaults for any field the backend leaves unset.
/// </summary>
internal static class S3BucketMapper
{
    /// <summary>
    /// Map an AWS bucket to its domain representation.
    /// </summary>
    /// <param name="bucket">The SDK bucket to map.</param>
    /// <returns>The domain bucket.</returns>
    public static S3Bucket ToBucket(Amazon.S3.Model.S3Bucket bucket)
        => new(
            bucket.BucketName ?? string.Empty,
            bucket.CreationDate?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty);

    /// <summary>
    /// Map an AWS object summary to its domain representation.
    /// </summary>
    /// <param name="s3Object">The SDK object to map.</param>
    /// <returns>The domain object.</returns>
    public static S3Object ToObject(Amazon.S3.Model.S3Object s3Object)
        => new(
            s3Object.Key ?? string.Empty,
            s3Object.Size ?? 0,
            s3Object.LastModified?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty);
}
