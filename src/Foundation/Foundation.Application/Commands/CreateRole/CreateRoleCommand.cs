using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.CreateRole;

/// <summary>
/// Create an IAM role with the supplied name, trust policy, and optional path, description, and session duration.
/// </summary>
/// <param name="RoleName">The name of the role to create.</param>
/// <param name="Path">The optional path for the role, or <see langword="null"/> for the default path.</param>
/// <param name="AssumeRolePolicyDocument">The trust policy JSON document that controls who may assume the role.</param>
/// <param name="Description">The optional description for the role, or <see langword="null"/> for none.</param>
/// <param name="MaxSessionDuration">The optional maximum session duration in seconds, or <see langword="null"/> for the default.</param>
public record CreateRoleCommand(string RoleName, string? Path, string AssumeRolePolicyDocument, string? Description, int? MaxSessionDuration) : ICommand;
