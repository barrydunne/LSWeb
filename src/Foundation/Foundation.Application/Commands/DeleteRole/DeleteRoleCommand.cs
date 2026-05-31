using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteRole;

/// <summary>
/// Delete an IAM role, first detaching its managed policies and deleting its inline policies.
/// </summary>
/// <param name="RoleName">The name of the role to delete.</param>
public record DeleteRoleCommand(string RoleName) : ICommand;
