using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeletePolicy;

/// <summary>
/// Delete a customer managed policy. The policy must not be attached to any principals.
/// </summary>
/// <param name="PolicyArn">The ARN of the policy to delete.</param>
public record DeletePolicyCommand(string PolicyArn) : ICommand;
