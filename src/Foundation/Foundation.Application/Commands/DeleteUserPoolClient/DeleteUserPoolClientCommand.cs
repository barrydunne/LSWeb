using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteUserPoolClient;

/// <summary>
/// Delete an Amazon Cognito user pool app client by its identifier.
/// </summary>
/// <param name="UserPoolId">The identifier of the user pool the app client belongs to.</param>
/// <param name="ClientId">The unique identifier of the app client to delete.</param>
public record DeleteUserPoolClientCommand(string UserPoolId, string ClientId) : ICommand;
