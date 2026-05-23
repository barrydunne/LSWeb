using AspNet.KickStarter.CQRS.Abstractions.Queries;

namespace Foundation.Application.Queries.PresignS3Object;

/// <summary>
/// Generate a time-limited presigned GET URL for a single S3 object.
/// </summary>
/// <param name="BucketName">The bucket the object lives in.</param>
/// <param name="Key">The full key of the object within the bucket.</param>
/// <param name="ExpirySeconds">The requested lifetime of the URL in seconds; clamped to a safe range.</param>
public record PresignS3ObjectQuery(string BucketName, string Key, int ExpirySeconds) : IQuery<PresignS3ObjectQueryResult>;

/// <summary>
/// A generated presigned URL and the effective expiry that was applied.
/// </summary>
/// <param name="Url">The presigned URL clients can use to fetch the object directly.</param>
/// <param name="ExpirySeconds">The effective lifetime of the URL in seconds after clamping.</param>
public record PresignS3ObjectQueryResult(string Url, int ExpirySeconds);
