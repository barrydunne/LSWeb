namespace Foundation.Domain.Cognito;

/// <summary>
/// The desired configuration of an Amazon Cognito user pool app client, used when creating or
/// updating it.
/// </summary>
/// <param name="UserPoolId">The identifier of the user pool the app client belongs to.</param>
/// <param name="ClientName">The name of the app client.</param>
/// <param name="GenerateSecret">Whether to generate a client secret. Ignored on update.</param>
/// <param name="ExplicitAuthFlows">The authentication flows the app client is permitted to use.</param>
/// <param name="AllowedOAuthFlows">The OAuth flows the app client is permitted to use (for example <c>code</c> or <c>implicit</c>).</param>
/// <param name="AllowedOAuthScopes">The OAuth scopes the app client may request (for example <c>openid</c> or <c>email</c>).</param>
/// <param name="CallbackURLs">The allowed callback URLs for the app client.</param>
/// <param name="AllowedOAuthFlowsUserPoolClient">Whether the app client participates in the hosted OAuth flows.</param>
/// <param name="ClientId">The identifier of the app client to update, or <see langword="null"/> when creating a new client.</param>
public sealed record UserPoolClientSpecification(
    string UserPoolId,
    string ClientName,
    bool GenerateSecret,
    IReadOnlyList<string> ExplicitAuthFlows,
    IReadOnlyList<string> AllowedOAuthFlows,
    IReadOnlyList<string> AllowedOAuthScopes,
    IReadOnlyList<string> CallbackURLs,
    bool AllowedOAuthFlowsUserPoolClient,
    string? ClientId = null);
