using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.MoveS3Object;

/// <summary>
/// Move a single S3 object to a destination key, optionally in a different bucket, by copying it
/// and then deleting the source object.
/// </summary>
/// <param name="SourceBucketName">The bucket the source object lives in.</param>
/// <param name="SourceKey">The full key of the source object.</param>
/// <param name="DestinationBucketName">The bucket the object is moved into.</param>
/// <param name="DestinationKey">The full key of the moved object.</param>
public record MoveS3ObjectCommand(
    string SourceBucketName,
    string SourceKey,
    string DestinationBucketName,
    string DestinationKey) : ICommand;
