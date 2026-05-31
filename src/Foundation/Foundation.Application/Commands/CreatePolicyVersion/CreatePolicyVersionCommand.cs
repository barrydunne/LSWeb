using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.CreatePolicyVersion;

/// <summary>
/// Create a new version of a customer managed policy, optionally promoting it to the default version.
/// </summary>
/// <param name="PolicyArn">The ARN of the policy to add a version to.</param>
/// <param name="PolicyDocument">The JSON policy document for the new version.</param>
/// <param name="SetAsDefault">Whether the new version should become the policy's default version.</param>
public record CreatePolicyVersionCommand(string PolicyArn, string PolicyDocument, bool SetAsDefault) : ICommand;
