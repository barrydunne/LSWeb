using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DetachRolePolicy;

/// <summary>
/// Detach a managed policy from an IAM role.
/// </summary>
/// <param name="RoleName">The name of the role to detach the policy from.</param>
/// <param name="PolicyArn">The ARN of the managed policy to detach.</param>
public record DetachRolePolicyCommand(string RoleName, string PolicyArn) : ICommand;
