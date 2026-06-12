using AspNet.KickStarter.CQRS.Abstractions.Commands;
using Foundation.Domain.Cognito;

namespace Foundation.Application.Commands.CreateCognitoUser;

/// <summary>
/// Create an Amazon Cognito user within a user pool from the supplied configuration.
/// </summary>
/// <param name="UserPoolId">The identifier of the user pool the user belongs to.</param>
/// <param name="Username">The unique username of the user to create.</param>
/// <param name="Attributes">The attributes to record against the user.</param>
/// <param name="TemporaryPassword">An optional temporary password to assign to the new user.</param>
public record CreateCognitoUserCommand(
    string UserPoolId,
    string Username,
    IReadOnlyList<CognitoUserAttributeEntry> Attributes,
    string? TemporaryPassword) : ICommand<CognitoUserDetail>;
