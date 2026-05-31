using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.AttachUserPolicy;

/// <summary>
/// Attach a managed policy to an IAM user.
/// </summary>
/// <param name="UserName">The name of the user to attach the policy to.</param>
/// <param name="PolicyArn">The ARN of the managed policy to attach.</param>
public record AttachUserPolicyCommand(string UserName, string PolicyArn) : ICommand;
