using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteS3BucketPolicy;

/// <summary>
/// Remove the access policy from an S3 bucket.
/// </summary>
/// <param name="BucketName">The bucket to remove the policy from.</param>
public record DeleteS3BucketPolicyCommand(string BucketName) : ICommand;
