using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteS3Bucket;

/// <summary>
/// Delete an S3 bucket.
/// </summary>
/// <param name="BucketName">The name of the bucket to delete.</param>
public record DeleteS3BucketCommand(string BucketName) : ICommand;
