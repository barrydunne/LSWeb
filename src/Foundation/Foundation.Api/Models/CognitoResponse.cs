namespace Foundation.Api.Models;

/// <summary>
/// The Amazon Cognito user pools available on the backend.
/// </summary>
/// <param name="UserPools">The user pool summaries, ordered as returned by the backend.</param>
public sealed record UserPoolListResponse(
    IReadOnlyList<UserPoolSummaryResponse> UserPools);

/// <summary>
/// A concise view of an Amazon Cognito user pool as it appears in a list.
/// </summary>
/// <param name="Id">The unique identifier of the user pool.</param>
/// <param name="Name">The human-readable name of the user pool.</param>
/// <param name="CreationDate">The moment the user pool was created, or <see langword="null"/> when not reported.</param>
public sealed record UserPoolSummaryResponse(
    string Id,
    string Name,
    DateTimeOffset? CreationDate);

/// <summary>
/// The full configuration of an Amazon Cognito user pool.
/// </summary>
/// <param name="Id">The unique identifier of the user pool.</param>
/// <param name="Name">The human-readable name of the user pool.</param>
/// <param name="Arn">The Amazon Resource Name of the user pool, or <see langword="null"/> when not reported.</param>
/// <param name="MfaConfiguration">The multi-factor authentication configuration, or <see langword="null"/> when not reported.</param>
/// <param name="EstimatedNumberOfUsers">The estimated number of users in the pool, or <see langword="null"/> when not reported.</param>
/// <param name="UsernameAttributes">The attributes that may be used as a username when signing in.</param>
/// <param name="AutoVerifiedAttributes">The attributes that Cognito automatically verifies.</param>
/// <param name="CreationDate">The moment the user pool was created, or <see langword="null"/> when not reported.</param>
/// <param name="LastModifiedDate">The moment the user pool was last modified, or <see langword="null"/> when not reported.</param>
public sealed record UserPoolDetailResponse(
    string Id,
    string Name,
    string? Arn,
    string? MfaConfiguration,
    int? EstimatedNumberOfUsers,
    IReadOnlyList<string> UsernameAttributes,
    IReadOnlyList<string> AutoVerifiedAttributes,
    DateTimeOffset? CreationDate,
    DateTimeOffset? LastModifiedDate);

/// <summary>
/// The configuration supplied when creating an Amazon Cognito user pool.
/// </summary>
/// <param name="Name">The name of the user pool to create.</param>
/// <param name="MfaConfiguration">The multi-factor authentication configuration (<c>OFF</c>, <c>ON</c> or <c>OPTIONAL</c>), or <see langword="null"/> to use the backend default.</param>
/// <param name="UsernameAttributes">The attributes that may be used as a username when signing in, or <see langword="null"/> for none.</param>
/// <param name="AutoVerifiedAttributes">The attributes that Cognito automatically verifies, or <see langword="null"/> for none.</param>
public sealed record UserPoolCreateRequest(
    string Name,
    string? MfaConfiguration,
    IReadOnlyList<string>? UsernameAttributes,
    IReadOnlyList<string>? AutoVerifiedAttributes);

/// <summary>
/// The identifier of a newly created Amazon Cognito user pool.
/// </summary>
/// <param name="Id">The unique identifier assigned to the created user pool.</param>
public sealed record UserPoolCreatedResponse(
    string Id);

/// <summary>
/// The app clients configured within an Amazon Cognito user pool.
/// </summary>
/// <param name="Clients">The app client summaries, ordered as returned by the backend.</param>
public sealed record UserPoolClientListResponse(
    IReadOnlyList<UserPoolClientSummaryResponse> Clients);

/// <summary>
/// A concise view of an Amazon Cognito user pool app client as it appears in a list.
/// </summary>
/// <param name="ClientId">The unique identifier of the app client.</param>
/// <param name="ClientName">The human-readable name of the app client.</param>
/// <param name="UserPoolId">The identifier of the user pool the app client belongs to.</param>
public sealed record UserPoolClientSummaryResponse(
    string ClientId,
    string ClientName,
    string UserPoolId);

/// <summary>
/// The full configuration of an Amazon Cognito user pool app client.
/// </summary>
/// <param name="ClientId">The unique identifier of the app client.</param>
/// <param name="ClientName">The human-readable name of the app client.</param>
/// <param name="UserPoolId">The identifier of the user pool the app client belongs to.</param>
/// <param name="ClientSecret">The generated client secret, or <see langword="null"/> when the client has no secret.</param>
/// <param name="GenerateSecret">Whether the app client was created with a generated secret.</param>
/// <param name="ExplicitAuthFlows">The authentication flows the app client is permitted to use.</param>
/// <param name="AllowedOAuthFlows">The OAuth flows the app client is permitted to use.</param>
/// <param name="AllowedOAuthScopes">The OAuth scopes the app client may request.</param>
/// <param name="CallbackURLs">The allowed callback URLs for the app client.</param>
/// <param name="AllowedOAuthFlowsUserPoolClient">Whether the app client participates in the hosted OAuth flows.</param>
/// <param name="CreationDate">The moment the app client was created, or <see langword="null"/> when not reported.</param>
/// <param name="LastModifiedDate">The moment the app client was last modified, or <see langword="null"/> when not reported.</param>
public sealed record UserPoolClientDetailResponse(
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

/// <summary>
/// The configuration supplied when creating an Amazon Cognito user pool app client.
/// </summary>
/// <param name="ClientName">The name of the app client to create.</param>
/// <param name="GenerateSecret">Whether to generate a client secret for the app client.</param>
/// <param name="ExplicitAuthFlows">The authentication flows the app client is permitted to use, or <see langword="null"/> for none.</param>
/// <param name="AllowedOAuthFlows">The OAuth flows the app client is permitted to use, or <see langword="null"/> for none.</param>
/// <param name="AllowedOAuthScopes">The OAuth scopes the app client may request, or <see langword="null"/> for none.</param>
/// <param name="CallbackURLs">The allowed callback URLs for the app client, or <see langword="null"/> for none.</param>
/// <param name="AllowedOAuthFlowsUserPoolClient">Whether the app client participates in the hosted OAuth flows.</param>
public sealed record UserPoolClientCreateRequest(
    string ClientName,
    bool GenerateSecret,
    IReadOnlyList<string>? ExplicitAuthFlows,
    IReadOnlyList<string>? AllowedOAuthFlows,
    IReadOnlyList<string>? AllowedOAuthScopes,
    IReadOnlyList<string>? CallbackURLs,
    bool AllowedOAuthFlowsUserPoolClient);

/// <summary>
/// The configuration supplied when updating an Amazon Cognito user pool app client.
/// </summary>
/// <param name="ClientName">The name of the app client.</param>
/// <param name="ExplicitAuthFlows">The authentication flows the app client is permitted to use, or <see langword="null"/> for none.</param>
/// <param name="AllowedOAuthFlows">The OAuth flows the app client is permitted to use, or <see langword="null"/> for none.</param>
/// <param name="AllowedOAuthScopes">The OAuth scopes the app client may request, or <see langword="null"/> for none.</param>
/// <param name="CallbackURLs">The allowed callback URLs for the app client, or <see langword="null"/> for none.</param>
/// <param name="AllowedOAuthFlowsUserPoolClient">Whether the app client participates in the hosted OAuth flows.</param>
public sealed record UserPoolClientUpdateRequest(
    string ClientName,
    IReadOnlyList<string>? ExplicitAuthFlows,
    IReadOnlyList<string>? AllowedOAuthFlows,
    IReadOnlyList<string>? AllowedOAuthScopes,
    IReadOnlyList<string>? CallbackURLs,
    bool AllowedOAuthFlowsUserPoolClient);
