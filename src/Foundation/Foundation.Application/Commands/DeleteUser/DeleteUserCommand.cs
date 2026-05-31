using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteUser;

/// <summary>
/// Delete an IAM user, first removing its access keys, inline policies, group memberships, and
/// detaching its managed policies so the delete is not blocked by dependent resources.
/// </summary>
/// <param name="UserName">The name of the user to delete.</param>
public record DeleteUserCommand(string UserName) : ICommand;
