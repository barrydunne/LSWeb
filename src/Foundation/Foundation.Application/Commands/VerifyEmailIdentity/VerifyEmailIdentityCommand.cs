using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.VerifyEmailIdentity;

/// <summary>
/// Start the verification of an SES email address identity.
/// </summary>
/// <param name="EmailAddress">The email address to verify.</param>
public record VerifyEmailIdentityCommand(string EmailAddress) : ICommand;
