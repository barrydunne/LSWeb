using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteGroup;

/// <summary>
/// Delete an IAM group, first removing its members, detaching its managed policies, and deleting its
/// inline policies.
/// </summary>
/// <param name="GroupName">The name of the group to delete.</param>
public record DeleteGroupCommand(string GroupName) : ICommand;
