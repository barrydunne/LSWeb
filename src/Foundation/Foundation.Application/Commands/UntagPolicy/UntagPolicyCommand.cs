using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.UntagPolicy;

/// <summary>
/// Remove tags from an IAM managed policy by key.
/// </summary>
/// <param name="PolicyArn">The ARN of the policy to untag.</param>
/// <param name="TagKeys">The keys of the tags to remove.</param>
public record UntagPolicyCommand(string PolicyArn, IReadOnlyList<string> TagKeys) : ICommand;
