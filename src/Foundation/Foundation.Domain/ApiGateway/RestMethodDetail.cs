namespace Foundation.Domain.ApiGateway;

/// <summary>
/// The configuration of a single HTTP method on an API Gateway REST API resource.
/// </summary>
/// <param name="ResourceId">The identifier of the resource the method belongs to.</param>
/// <param name="HttpMethod">The HTTP verb of the method (for example <c>GET</c>, <c>POST</c> or <c>ANY</c>).</param>
/// <param name="AuthorizationType">The authorization type of the method (for example <c>NONE</c> or <c>COGNITO_USER_POOLS</c>).</param>
/// <param name="AuthorizerId">The identifier of the authorizer applied to the method, or <see langword="null"/> when none is set.</param>
/// <param name="ApiKeyRequired">Whether an API key is required to call the method.</param>
/// <param name="AuthorizationScopes">The authorization scopes required by the method, when applicable.</param>
public sealed record RestMethodDetail(
    string ResourceId,
    string HttpMethod,
    string AuthorizationType,
    string? AuthorizerId,
    bool ApiKeyRequired,
    IReadOnlyList<string> AuthorizationScopes);
