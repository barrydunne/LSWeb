using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.S3;

namespace Foundation.Application.Queries.ListS3ObjectVersions;

/// <summary>
/// List the object versions in an S3 bucket.
/// </summary>
/// <param name="BucketName">The bucket to list versions in.</param>
/// <param name="Prefix">The key prefix to filter by; empty for the whole bucket.</param>
public record ListS3ObjectVersionsQuery(string BucketName, string Prefix) : IQuery<ListS3ObjectVersionsQueryResult>;

/// <summary>
/// The object versions in the requested bucket.
/// </summary>
/// <param name="Versions">The object versions.</param>
public record ListS3ObjectVersionsQueryResult(IReadOnlyList<S3ObjectVersion> Versions);
