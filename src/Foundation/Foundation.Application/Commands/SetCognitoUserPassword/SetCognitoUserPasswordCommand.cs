using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.SetCognitoUserPassword;

/// <summary>
/// Set the password of an Amazon Cognito user, optionally marking it as permanent.
/// </summary>
/// <param name="UserPoolId">The identifier of the user pool the user belongs to.</param>
/// <param name="Username">The unique username of the user.</param>
/// <param name="Password">The new password to assign.</param>
/// <param name="Permanent">Whether the password is permanent rather than a temporary one.</param>
public record SetCognitoUserPasswordCommand(
    string UserPoolId,
    string Username,
    string Password,
    bool Permanent) : ICommand;
