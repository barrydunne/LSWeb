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
/// <param name="PasswordPolicy">The password complexity rules enforced by the pool, or <see langword="null"/> when not reported.</param>
public sealed record UserPoolDetailResponse(
    string Id,
    string Name,
    string? Arn,
    string? MfaConfiguration,
    int? EstimatedNumberOfUsers,
    IReadOnlyList<string> UsernameAttributes,
    IReadOnlyList<string> AutoVerifiedAttributes,
    DateTimeOffset? CreationDate,
    DateTimeOffset? LastModifiedDate,
    PasswordPolicyModel? PasswordPolicy);

/// <summary>
/// The password complexity rules of an Amazon Cognito user pool.
/// </summary>
/// <param name="MinimumLength">The minimum number of characters a password must contain.</param>
/// <param name="RequireUppercase">Whether passwords must contain at least one uppercase letter.</param>
/// <param name="RequireLowercase">Whether passwords must contain at least one lowercase letter.</param>
/// <param name="RequireNumbers">Whether passwords must contain at least one digit.</param>
/// <param name="RequireSymbols">Whether passwords must contain at least one symbol.</param>
public sealed record PasswordPolicyModel(
    int MinimumLength,
    bool RequireUppercase,
    bool RequireLowercase,
    bool RequireNumbers,
    bool RequireSymbols);

/// <summary>
/// The configuration supplied when creating an Amazon Cognito user pool.
/// </summary>
/// <param name="Name">The name of the user pool to create.</param>
/// <param name="MfaConfiguration">The multi-factor authentication configuration (<c>OFF</c>, <c>ON</c> or <c>OPTIONAL</c>), or <see langword="null"/> to use the backend default.</param>
/// <param name="UsernameAttributes">The attributes that may be used as a username when signing in, or <see langword="null"/> for none.</param>
/// <param name="AutoVerifiedAttributes">The attributes that Cognito automatically verifies, or <see langword="null"/> for none.</param>
/// <param name="PasswordPolicy">The password complexity rules to enforce, or <see langword="null"/> to use the backend default.</param>
public sealed record UserPoolCreateRequest(
    string Name,
    string? MfaConfiguration,
    IReadOnlyList<string>? UsernameAttributes,
    IReadOnlyList<string>? AutoVerifiedAttributes,
    PasswordPolicyModel? PasswordPolicy);

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

/// <summary>
/// The users within an Amazon Cognito user pool.
/// </summary>
/// <param name="Users">The user summaries, ordered as returned by the backend.</param>
public sealed record CognitoUserListResponse(
    IReadOnlyList<CognitoUserSummaryResponse> Users);

/// <summary>
/// A concise view of an Amazon Cognito user as it appears in a list.
/// </summary>
/// <param name="Username">The unique username of the user within the pool.</param>
/// <param name="Status">The account status of the user.</param>
/// <param name="Enabled">Whether the user account is enabled and able to sign in.</param>
/// <param name="CreatedDate">The moment the user was created, or <see langword="null"/> when not reported.</param>
public sealed record CognitoUserSummaryResponse(
    string Username,
    string Status,
    bool Enabled,
    DateTimeOffset? CreatedDate);

/// <summary>
/// A single attribute of an Amazon Cognito user.
/// </summary>
/// <param name="Name">The name of the attribute.</param>
/// <param name="Value">The value of the attribute.</param>
public sealed record CognitoUserAttributeResponse(
    string Name,
    string Value);

/// <summary>
/// The full configuration of an Amazon Cognito user.
/// </summary>
/// <param name="Username">The unique username of the user within the pool.</param>
/// <param name="Status">The account status of the user.</param>
/// <param name="Enabled">Whether the user account is enabled and able to sign in.</param>
/// <param name="Attributes">The attributes recorded against the user.</param>
/// <param name="CreatedDate">The moment the user was created, or <see langword="null"/> when not reported.</param>
/// <param name="LastModifiedDate">The moment the user was last modified, or <see langword="null"/> when not reported.</param>
public sealed record CognitoUserDetailResponse(
    string Username,
    string Status,
    bool Enabled,
    IReadOnlyList<CognitoUserAttributeResponse> Attributes,
    DateTimeOffset? CreatedDate,
    DateTimeOffset? LastModifiedDate);

/// <summary>
/// A single attribute supplied when creating an Amazon Cognito user.
/// </summary>
/// <param name="Name">The name of the attribute.</param>
/// <param name="Value">The value of the attribute.</param>
public sealed record CognitoUserAttributeRequest(
    string Name,
    string Value);

/// <summary>
/// The configuration supplied when creating an Amazon Cognito user.
/// </summary>
/// <param name="Username">The unique username of the user to create.</param>
/// <param name="Attributes">The attributes to record against the user, or <see langword="null"/> for none.</param>
/// <param name="TemporaryPassword">An optional temporary password to assign to the new user.</param>
public sealed record CognitoUserCreateRequest(
    string Username,
    IReadOnlyList<CognitoUserAttributeRequest>? Attributes,
    string? TemporaryPassword);

/// <summary>
/// The configuration supplied when setting an Amazon Cognito user's password.
/// </summary>
/// <param name="Password">The new password to assign.</param>
/// <param name="Permanent">Whether the password is permanent rather than a temporary one.</param>
public sealed record CognitoUserPasswordRequest(
    string Password,
    bool Permanent);

/// <summary>
/// The configuration supplied when enabling or disabling an Amazon Cognito user.
/// </summary>
/// <param name="Enabled">Whether the account should be enabled.</param>
public sealed record CognitoUserEnabledRequest(
    bool Enabled);

/// <summary>
/// The credentials supplied when requesting bearer tokens for an Amazon Cognito app client.
/// </summary>
/// <param name="Username">The username to authenticate.</param>
/// <param name="Password">The password to authenticate with.</param>
public sealed record CognitoTokenRequest(
    string Username,
    string Password);

/// <summary>
/// The tokens issued for an Amazon Cognito app client and the decoded identity token claims.
/// </summary>
/// <param name="AccessToken">The issued access token, or <see langword="null"/> when none was issued.</param>
/// <param name="IdToken">The issued identity token, or <see langword="null"/> when none was issued.</param>
/// <param name="RefreshToken">The issued refresh token, or <see langword="null"/> when none was issued.</param>
/// <param name="TokenType">The type of the issued tokens, or <see langword="null"/> when not reported.</param>
/// <param name="ExpiresIn">The number of seconds until the access token expires, or <see langword="null"/> when not reported.</param>
/// <param name="Claims">The claims decoded from the identity token.</param>
public sealed record CognitoTokenResponse(
    string? AccessToken,
    string? IdToken,
    string? RefreshToken,
    string? TokenType,
    int? ExpiresIn,
    IReadOnlyList<CognitoUserAttributeResponse> Claims);
