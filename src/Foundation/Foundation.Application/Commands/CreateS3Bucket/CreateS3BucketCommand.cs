using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.CreateS3Bucket;

/// <summary>
/// Create a new S3 bucket.
/// </summary>
/// <param name="BucketName">The name of the bucket to create.</param>
public record CreateS3BucketCommand(string BucketName) : ICommand;
