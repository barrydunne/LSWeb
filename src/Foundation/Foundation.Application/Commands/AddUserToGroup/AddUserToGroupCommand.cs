using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.AddUserToGroup;

/// <summary>
/// Add an IAM user to a group.
/// </summary>
/// <param name="UserName">The name of the user to add.</param>
/// <param name="GroupName">The name of the group to add the user to.</param>
public record AddUserToGroupCommand(string UserName, string GroupName) : ICommand;
