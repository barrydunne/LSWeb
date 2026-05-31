using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.PutUserInlinePolicy;

/// <summary>
/// Create or replace an inline policy on an IAM user.
/// </summary>
/// <param name="UserName">The name of the user to put the inline policy on.</param>
/// <param name="PolicyName">The name of the inline policy.</param>
/// <param name="PolicyDocument">The JSON policy document.</param>
public record PutUserInlinePolicyCommand(string UserName, string PolicyName, string PolicyDocument) : ICommand;
