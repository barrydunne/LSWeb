using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteS3Object;

/// <summary>
/// Delete a single object from an S3 bucket.
/// </summary>
/// <param name="BucketName">The bucket the object lives in.</param>
/// <param name="Key">The full key of the object within the bucket.</param>
public record DeleteS3ObjectCommand(string BucketName, string Key) : ICommand;
