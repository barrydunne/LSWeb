using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DetachGroupPolicy;

/// <summary>
/// Detach a managed policy from an IAM group.
/// </summary>
/// <param name="GroupName">The name of the group to detach the policy from.</param>
/// <param name="PolicyArn">The ARN of the managed policy to detach.</param>
public record DetachGroupPolicyCommand(string GroupName, string PolicyArn) : ICommand;
