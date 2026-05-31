using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.RemoveUserFromGroup;

/// <summary>
/// Remove an IAM user from a group.
/// </summary>
/// <param name="UserName">The name of the user to remove.</param>
/// <param name="GroupName">The name of the group to remove the user from.</param>
public record RemoveUserFromGroupCommand(string UserName, string GroupName) : ICommand;
