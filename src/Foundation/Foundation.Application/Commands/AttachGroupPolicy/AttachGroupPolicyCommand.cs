using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.AttachGroupPolicy;

/// <summary>
/// Attach a managed policy to an IAM group.
/// </summary>
/// <param name="GroupName">The name of the group to attach the policy to.</param>
/// <param name="PolicyArn">The ARN of the managed policy to attach.</param>
public record AttachGroupPolicyCommand(string GroupName, string PolicyArn) : ICommand;
