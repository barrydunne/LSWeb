using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteSesIdentity;

/// <summary>
/// Delete an SES identity (an email address or domain) from the backend.
/// </summary>
/// <param name="Identity">The email address or domain name to delete.</param>
public record DeleteSesIdentityCommand(string Identity) : ICommand;
