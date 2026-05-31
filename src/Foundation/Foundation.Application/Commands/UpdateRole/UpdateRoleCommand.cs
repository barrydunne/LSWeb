using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.UpdateRole;

/// <summary>
/// Update the description and maximum session duration of an IAM role.
/// </summary>
/// <param name="RoleName">The name of the role to update.</param>
/// <param name="Description">The optional new description, or <see langword="null"/> to leave it unchanged.</param>
/// <param name="MaxSessionDuration">The optional new maximum session duration in seconds, or <see langword="null"/> to leave it unchanged.</param>
public record UpdateRoleCommand(string RoleName, string? Description, int? MaxSessionDuration) : ICommand;
