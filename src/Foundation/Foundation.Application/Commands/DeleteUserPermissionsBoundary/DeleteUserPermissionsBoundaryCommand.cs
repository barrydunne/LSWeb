using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteUserPermissionsBoundary;

/// <summary>
/// Remove the permissions boundary from an IAM user.
/// </summary>
/// <param name="UserName">The name of the user to remove the boundary from.</param>
public record DeleteUserPermissionsBoundaryCommand(string UserName) : ICommand;
