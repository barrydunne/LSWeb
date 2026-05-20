using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.S3;

namespace Foundation.Application.Queries.ListS3Buckets;

/// <summary>
/// List the S3 buckets available on the configured backend.
/// </summary>
public record ListS3BucketsQuery : IQuery<ListS3BucketsQueryResult>;

/// <summary>
/// The S3 buckets returned by the backend.
/// </summary>
/// <param name="Buckets">The buckets, ordered as returned by the backend.</param>
public record ListS3BucketsQueryResult(IReadOnlyList<S3Bucket> Buckets);
