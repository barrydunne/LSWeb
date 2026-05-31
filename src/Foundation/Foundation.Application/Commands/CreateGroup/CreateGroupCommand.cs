using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.CreateGroup;

/// <summary>
/// Create an IAM group with the supplied name and optional path.
/// </summary>
/// <param name="GroupName">The name of the group to create.</param>
/// <param name="Path">The optional path for the group, or <see langword="null"/> for the default path.</param>
public record CreateGroupCommand(string GroupName, string? Path) : ICommand;
