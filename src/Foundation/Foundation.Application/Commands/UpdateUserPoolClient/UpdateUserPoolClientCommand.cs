using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.UpdateUserPoolClient;

/// <summary>
/// Update the configuration of an existing Amazon Cognito user pool app client.
/// </summary>
/// <param name="UserPoolId">The identifier of the user pool the app client belongs to.</param>
/// <param name="ClientId">The unique identifier of the app client to update.</param>
/// <param name="ClientName">The name of the app client.</param>
/// <param name="ExplicitAuthFlows">The authentication flows the app client is permitted to use.</param>
/// <param name="AllowedOAuthFlows">The OAuth flows the app client is permitted to use.</param>
/// <param name="AllowedOAuthScopes">The OAuth scopes the app client may request.</param>
/// <param name="CallbackURLs">The allowed callback URLs for the app client.</param>
/// <param name="AllowedOAuthFlowsUserPoolClient">Whether the app client participates in the hosted OAuth flows.</param>
public record UpdateUserPoolClientCommand(
    string UserPoolId,
    string ClientId,
    string ClientName,
    IReadOnlyList<string> ExplicitAuthFlows,
    IReadOnlyList<string> AllowedOAuthFlows,
    IReadOnlyList<string> AllowedOAuthScopes,
    IReadOnlyList<string> CallbackURLs,
    bool AllowedOAuthFlowsUserPoolClient) : ICommand;
