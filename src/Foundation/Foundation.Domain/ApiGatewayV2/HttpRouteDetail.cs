namespace Foundation.Domain.ApiGatewayV2;

/// <summary>
/// The full configuration of an Amazon API Gateway v2 route.
/// </summary>
/// <param name="RouteId">The unique identifier of the route.</param>
/// <param name="RouteKey">The route key (for example <c>GET /items</c> or <c>$default</c>).</param>
/// <param name="Target">The integration target of the route (for example <c>integrations/abc123</c>), or <see langword="null"/> when none is attached.</param>
/// <param name="AuthorizationType">The authorization type of the route (for example <c>NONE</c> or <c>JWT</c>), or <see langword="null"/> when not reported.</param>
/// <param name="AuthorizerId">The identifier of the authorizer attached to the route, or <see langword="null"/> when none is attached.</param>
/// <param name="AuthorizationScopes">The authorization scopes required by the route.</param>
/// <param name="ApiKeyRequired">Whether an API key is required to call the route, or <see langword="null"/> when not reported.</param>
public sealed record HttpRouteDetail(
    string RouteId,
    string RouteKey,
    string? Target,
    string? AuthorizationType,
    string? AuthorizerId,
    IReadOnlyList<string> AuthorizationScopes,
    bool? ApiKeyRequired);
