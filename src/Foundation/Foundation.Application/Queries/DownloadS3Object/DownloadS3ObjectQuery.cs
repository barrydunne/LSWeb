using AspNet.KickStarter.CQRS.Abstractions.Queries;

namespace Foundation.Application.Queries.DownloadS3Object;

/// <summary>
/// Download the content of a single object from an S3 bucket.
/// </summary>
/// <param name="BucketName">The bucket the object lives in.</param>
/// <param name="Key">The full key of the object within the bucket.</param>
public record DownloadS3ObjectQuery(string BucketName, string Key) : IQuery<DownloadS3ObjectQueryResult>;

/// <summary>
/// The content of a single S3 object.
/// </summary>
/// <param name="Content">The object's bytes.</param>
/// <param name="ContentType">The reported content type.</param>
/// <param name="FileName">The suggested download file name derived from the key.</param>
public record DownloadS3ObjectQueryResult(byte[] Content, string ContentType, string FileName);
