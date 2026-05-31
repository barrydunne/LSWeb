using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.PutRoleInlinePolicy;

/// <summary>
/// Create or replace an inline policy on an IAM role.
/// </summary>
/// <param name="RoleName">The name of the role to apply the inline policy to.</param>
/// <param name="PolicyName">The name of the inline policy.</param>
/// <param name="PolicyDocument">The policy JSON document.</param>
public record PutRoleInlinePolicyCommand(string RoleName, string PolicyName, string PolicyDocument) : ICommand;
