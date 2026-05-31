using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteRoleInlinePolicy;

/// <summary>
/// Delete an inline policy from an IAM role.
/// </summary>
/// <param name="RoleName">The name of the role to remove the inline policy from.</param>
/// <param name="PolicyName">The name of the inline policy to delete.</param>
public record DeleteRoleInlinePolicyCommand(string RoleName, string PolicyName) : ICommand;
