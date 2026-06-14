using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.PutS3BucketPolicy;

/// <summary>
/// Apply an access policy to an S3 bucket, replacing any existing policy.
/// </summary>
/// <param name="BucketName">The bucket to apply the policy to.</param>
/// <param name="Policy">The policy document as JSON.</param>
public record PutS3BucketPolicyCommand(string BucketName, string Policy) : ICommand;
