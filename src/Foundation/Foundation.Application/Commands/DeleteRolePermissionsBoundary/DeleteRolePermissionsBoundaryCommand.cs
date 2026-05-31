using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteRolePermissionsBoundary;

/// <summary>
/// Remove the permissions boundary from an IAM role.
/// </summary>
/// <param name="RoleName">The name of the role to remove the boundary from.</param>
public record DeleteRolePermissionsBoundaryCommand(string RoleName) : ICommand;
