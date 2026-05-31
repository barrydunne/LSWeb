using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.AttachRolePolicy;

/// <summary>
/// Attach a managed policy to an IAM role.
/// </summary>
/// <param name="RoleName">The name of the role to attach the policy to.</param>
/// <param name="PolicyArn">The ARN of the managed policy to attach.</param>
public record AttachRolePolicyCommand(string RoleName, string PolicyArn) : ICommand;
