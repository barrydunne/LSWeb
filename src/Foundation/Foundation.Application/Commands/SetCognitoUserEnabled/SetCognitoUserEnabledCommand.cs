using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.SetCognitoUserEnabled;

/// <summary>
/// Enable or disable an Amazon Cognito user account.
/// </summary>
/// <param name="UserPoolId">The identifier of the user pool the user belongs to.</param>
/// <param name="Username">The unique username of the user.</param>
/// <param name="Enabled">Whether the account should be enabled.</param>
public record SetCognitoUserEnabledCommand(
    string UserPoolId,
    string Username,
    bool Enabled) : ICommand;
