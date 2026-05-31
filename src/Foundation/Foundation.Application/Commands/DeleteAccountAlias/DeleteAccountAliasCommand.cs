using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteAccountAlias;

/// <summary>
/// Delete an account alias.
/// </summary>
/// <param name="AccountAlias">The alias to delete.</param>
public record DeleteAccountAliasCommand(string AccountAlias) : ICommand;
