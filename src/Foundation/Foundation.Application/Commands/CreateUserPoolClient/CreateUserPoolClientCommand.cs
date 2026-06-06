using AspNet.KickStarter.CQRS.Abstractions.Commands;
using Foundation.Domain.Cognito;

namespace Foundation.Application.Commands.CreateUserPoolClient;

/// <summary>
/// Create an Amazon Cognito user pool app client from the supplied configuration.
/// </summary>
/// <param name="UserPoolId">The identifier of the user pool the app client belongs to.</param>
/// <param name="ClientName">The name of the app client to create.</param>
/// <param name="GenerateSecret">Whether to generate a client secret for the app client.</param>
/// <param name="ExplicitAuthFlows">The authentication flows the app client is permitted to use.</param>
/// <param name="AllowedOAuthFlows">The OAuth flows the app client is permitted to use.</param>
/// <param name="AllowedOAuthScopes">The OAuth scopes the app client may request.</param>
/// <param name="CallbackURLs">The allowed callback URLs for the app client.</param>
/// <param name="AllowedOAuthFlowsUserPoolClient">Whether the app client participates in the hosted OAuth flows.</param>
public record CreateUserPoolClientCommand(
    string UserPoolId,
    string ClientName,
    bool GenerateSecret,
    IReadOnlyList<string> ExplicitAuthFlows,
    IReadOnlyList<string> AllowedOAuthFlows,
    IReadOnlyList<string> AllowedOAuthScopes,
    IReadOnlyList<string> CallbackURLs,
    bool AllowedOAuthFlowsUserPoolClient) : ICommand<UserPoolClientDetail>;
