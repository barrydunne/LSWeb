using AspNet.KickStarter.CQRS.Abstractions.Commands;
using Foundation.Domain.Cognito;

namespace Foundation.Application.Commands.RegenerateUserPoolClientSecret;

/// <summary>
/// Regenerate the client secret of an Amazon Cognito user pool app client by recreating it.
/// </summary>
/// <param name="UserPoolId">The identifier of the user pool the app client belongs to.</param>
/// <param name="ClientId">The unique identifier of the app client whose secret should be regenerated.</param>
public record RegenerateUserPoolClientSecretCommand(
    string UserPoolId,
    string ClientId) : ICommand<UserPoolClientDetail>;
