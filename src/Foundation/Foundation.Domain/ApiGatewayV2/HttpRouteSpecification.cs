namespace Foundation.Domain.ApiGatewayV2;

/// <summary>
/// The desired configuration of an Amazon API Gateway v2 route when creating or updating one.
/// </summary>
/// <param name="ApiId">The identifier of the API the route belongs to.</param>
/// <param name="RouteId">The identifier of the route to update, or <see langword="null"/> when creating a new route.</param>
/// <param name="RouteKey">The route key (for example <c>GET /items</c> or <c>$default</c>).</param>
/// <param name="Target">The integration target of the route (for example <c>integrations/abc123</c>), or <see langword="null"/> for none.</param>
/// <param name="AuthorizationType">The authorization type of the route (for example <c>NONE</c> or <c>JWT</c>), or <see langword="null"/> to use the backend default.</param>
/// <param name="AuthorizerId">The identifier of the authorizer to attach to the route, or <see langword="null"/> for none.</param>
/// <param name="AuthorizationScopes">The authorization scopes required by the route.</param>
public sealed record HttpRouteSpecification(
    string ApiId,
    string? RouteId,
    string RouteKey,
    string? Target,
    string? AuthorizationType,
    string? AuthorizerId,
    IReadOnlyList<string> AuthorizationScopes);
