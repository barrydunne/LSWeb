namespace Foundation.Domain.ApiGateway;

/// <summary>
/// A detailed view of an API Gateway REST API authorizer.
/// </summary>
/// <param name="Id">The identifier that uniquely identifies the authorizer.</param>
/// <param name="Name">The name of the authorizer.</param>
/// <param name="Type">The authorizer type (for example <c>COGNITO_USER_POOLS</c>).</param>
/// <param name="ProviderARNs">The Cognito user pool ARNs the authorizer trusts.</param>
/// <param name="IdentitySource">The request location the identity token is read from, or <see langword="null"/> when not set.</param>
/// <param name="AuthType">An optional friendly authorization type label, or <see langword="null"/> when not set.</param>
public sealed record RestAuthorizerDetail(
    string Id,
    string Name,
    string Type,
    IReadOnlyList<string> ProviderARNs,
    string? IdentitySource,
    string? AuthType);
