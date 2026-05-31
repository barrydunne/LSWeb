using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.CreateAccountAlias;

/// <summary>
/// Create an account alias.
/// </summary>
/// <param name="AccountAlias">The alias to create.</param>
public record CreateAccountAliasCommand(string AccountAlias) : ICommand;
