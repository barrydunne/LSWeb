using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeletePolicyVersion;

/// <summary>
/// Delete a non-default version of a customer managed policy.
/// </summary>
/// <param name="PolicyArn">The ARN of the policy whose version to delete.</param>
/// <param name="VersionId">The identifier of the version to delete.</param>
public record DeletePolicyVersionCommand(string PolicyArn, string VersionId) : ICommand;
