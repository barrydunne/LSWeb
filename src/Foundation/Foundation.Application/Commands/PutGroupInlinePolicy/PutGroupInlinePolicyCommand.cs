using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.PutGroupInlinePolicy;

/// <summary>
/// Create or replace an inline policy on an IAM group.
/// </summary>
/// <param name="GroupName">The name of the group to put the inline policy on.</param>
/// <param name="PolicyName">The name of the inline policy.</param>
/// <param name="PolicyDocument">The JSON policy document.</param>
public record PutGroupInlinePolicyCommand(string GroupName, string PolicyName, string PolicyDocument) : ICommand;
