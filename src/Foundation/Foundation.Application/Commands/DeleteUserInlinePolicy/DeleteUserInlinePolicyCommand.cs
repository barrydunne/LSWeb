using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteUserInlinePolicy;

/// <summary>
/// Delete an inline policy from an IAM user.
/// </summary>
/// <param name="UserName">The name of the user to delete the inline policy from.</param>
/// <param name="PolicyName">The name of the inline policy to delete.</param>
public record DeleteUserInlinePolicyCommand(string UserName, string PolicyName) : ICommand;
