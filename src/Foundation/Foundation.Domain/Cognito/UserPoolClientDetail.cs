namespace Foundation.Domain.Cognito;

/// <summary>
/// The full configuration of an Amazon Cognito user pool app client, including its generated
/// credentials and OAuth settings. The client secret is only present when the client was created
/// with one.
/// </summary>
/// <param name="ClientId">The unique identifier of the app client.</param>
/// <param name="ClientName">The human-readable name of the app client.</param>
/// <param name="UserPoolId">The identifier of the user pool the app client belongs to.</param>
/// <param name="ClientSecret">The generated client secret, or <see langword="null"/> when the client has no secret.</param>
/// <param name="GenerateSecret">Whether the app client was created with a generated secret.</param>
/// <param name="ExplicitAuthFlows">The authentication flows the app client is permitted to use.</param>
/// <param name="AllowedOAuthFlows">The OAuth flows the app client is permitted to use (for example <c>code</c> or <c>implicit</c>).</param>
/// <param name="AllowedOAuthScopes">The OAuth scopes the app client may request (for example <c>openid</c> or <c>email</c>).</param>
/// <param name="CallbackURLs">The allowed callback URLs for the app client.</param>
/// <param name="AllowedOAuthFlowsUserPoolClient">Whether the app client participates in the hosted OAuth flows.</param>
/// <param name="CreationDate">The moment the app client was created, if reported.</param>
/// <param name="LastModifiedDate">The moment the app client was last modified, if reported.</param>
public sealed record UserPoolClientDetail(
    string ClientId,
    string ClientName,
    string UserPoolId,
    string? ClientSecret,
    bool GenerateSecret,
    IReadOnlyList<string> ExplicitAuthFlows,
    IReadOnlyList<string> AllowedOAuthFlows,
    IReadOnlyList<string> AllowedOAuthScopes,
    IReadOnlyList<string> CallbackURLs,
    bool AllowedOAuthFlowsUserPoolClient,
    DateTimeOffset? CreationDate,
    DateTimeOffset? LastModifiedDate);
