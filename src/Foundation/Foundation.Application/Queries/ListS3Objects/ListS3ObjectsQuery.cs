using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.S3;

namespace Foundation.Application.Queries.ListS3Objects;

/// <summary>
/// List the folders and objects directly beneath a prefix within an S3 bucket.
/// </summary>
/// <param name="BucketName">The bucket to browse.</param>
/// <param name="Prefix">The prefix to list beneath; empty lists the bucket root.</param>
public record ListS3ObjectsQuery(string BucketName, string Prefix) : IQuery<ListS3ObjectsQueryResult>;

/// <summary>
/// The folders and objects returned for a prefix.
/// </summary>
/// <param name="Prefixes">The immediate child prefixes (folders), each ending with a delimiter.</param>
/// <param name="Objects">The objects (files) that live directly under the requested prefix.</param>
public record ListS3ObjectsQueryResult(
    IReadOnlyList<string> Prefixes,
    IReadOnlyList<S3Object> Objects);
