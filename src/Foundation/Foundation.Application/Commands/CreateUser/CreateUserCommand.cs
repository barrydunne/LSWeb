using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.CreateUser;

/// <summary>
/// Create an IAM user with the supplied name and optional path.
/// </summary>
/// <param name="UserName">The name of the user to create.</param>
/// <param name="Path">The optional path for the user, or <see langword="null"/> for the default path.</param>
public record CreateUserCommand(string UserName, string? Path) : ICommand;
