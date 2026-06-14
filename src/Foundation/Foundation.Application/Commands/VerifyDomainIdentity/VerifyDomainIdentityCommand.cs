using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.VerifyDomainIdentity;

/// <summary>
/// Initiate the verification of an SES domain identity.
/// </summary>
/// <param name="Domain">The domain name to verify.</param>
public record VerifyDomainIdentityCommand(string Domain) : ICommand;
