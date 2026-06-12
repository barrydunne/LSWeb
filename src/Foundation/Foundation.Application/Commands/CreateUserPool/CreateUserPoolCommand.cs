using AspNet.KickStarter.CQRS.Abstractions.Commands;
using Foundation.Domain.Cognito;

namespace Foundation.Application.Commands.CreateUserPool;

/// <summary>
/// Create an Amazon Cognito user pool from the supplied configuration.
/// </summary>
/// <param name="Name">The name of the user pool to create.</param>
/// <param name="MfaConfiguration">The multi-factor authentication configuration (<c>OFF</c>, <c>ON</c> or <c>OPTIONAL</c>), or <see langword="null"/> to use the backend default.</param>
/// <param name="UsernameAttributes">The attributes that may be used as a username when signing in (for example <c>email</c> or <c>phone_number</c>).</param>
/// <param name="AutoVerifiedAttributes">The attributes that Cognito automatically verifies (for example <c>email</c> or <c>phone_number</c>).</param>
/// <param name="PasswordPolicy">The password complexity rules to enforce, or <see langword="null"/> to use the backend default.</param>
public record CreateUserPoolCommand(
    string Name,
    string? MfaConfiguration,
    IReadOnlyList<string> UsernameAttributes,
    IReadOnlyList<string> AutoVerifiedAttributes,
    PasswordPolicy? PasswordPolicy = null) : ICommand<string>;
