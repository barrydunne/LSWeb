using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteCognitoUser;

/// <summary>
/// Delete an Amazon Cognito user by its username.
/// </summary>
/// <param name="UserPoolId">The identifier of the user pool the user belongs to.</param>
/// <param name="Username">The unique username of the user to delete.</param>
public record DeleteCognitoUserCommand(string UserPoolId, string Username) : ICommand;
