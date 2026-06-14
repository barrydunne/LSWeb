using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.EnableDomainDkim;

/// <summary>
/// Enable DKIM signing for an SES domain identity.
/// </summary>
/// <param name="Domain">The domain name to enable DKIM for.</param>
public record EnableDomainDkimCommand(string Domain) : ICommand;
