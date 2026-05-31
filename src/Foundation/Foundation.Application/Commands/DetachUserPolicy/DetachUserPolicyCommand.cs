using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DetachUserPolicy;

/// <summary>
/// Detach a managed policy from an IAM user.
/// </summary>
/// <param name="UserName">The name of the user to detach the policy from.</param>
/// <param name="PolicyArn">The ARN of the managed policy to detach.</param>
public record DetachUserPolicyCommand(string UserName, string PolicyArn) : ICommand;
