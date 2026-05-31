using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteAccountPasswordPolicy;

/// <summary>
/// Delete the account password policy so that the backend default applies.
/// </summary>
public record DeleteAccountPasswordPolicyCommand : ICommand;
