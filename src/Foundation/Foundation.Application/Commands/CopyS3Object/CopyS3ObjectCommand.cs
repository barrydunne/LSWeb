using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.CopyS3Object;

/// <summary>
/// Copy a single S3 object to a destination key, optionally in a different bucket.
/// </summary>
/// <param name="SourceBucketName">The bucket the source object lives in.</param>
/// <param name="SourceKey">The full key of the source object.</param>
/// <param name="DestinationBucketName">The bucket the object is copied into.</param>
/// <param name="DestinationKey">The full key of the copied object.</param>
public record CopyS3ObjectCommand(
    string SourceBucketName,
    string SourceKey,
    string DestinationBucketName,
    string DestinationKey) : ICommand;
