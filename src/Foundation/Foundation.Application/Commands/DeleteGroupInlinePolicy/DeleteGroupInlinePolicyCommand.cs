using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteGroupInlinePolicy;

/// <summary>
/// Delete an inline policy from an IAM group.
/// </summary>
/// <param name="GroupName">The name of the group to delete the inline policy from.</param>
/// <param name="PolicyName">The name of the inline policy to delete.</param>
public record DeleteGroupInlinePolicyCommand(string GroupName, string PolicyName) : ICommand;
