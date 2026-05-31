using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.SetDefaultPolicyVersion;

/// <summary>
/// Promote an existing version of a customer managed policy to be the default version.
/// </summary>
/// <param name="PolicyArn">The ARN of the policy to update.</param>
/// <param name="VersionId">The identifier of the version to make the default.</param>
public record SetDefaultPolicyVersionCommand(string PolicyArn, string VersionId) : ICommand;
