namespace Foundation.Domain.ApiGatewayV2;

/// <summary>
/// A concise view of an Amazon API Gateway v2 route as it appears in a list.
/// </summary>
/// <param name="RouteId">The unique identifier of the route.</param>
/// <param name="RouteKey">The route key (for example <c>GET /items</c> or <c>$default</c>).</param>
/// <param name="Target">The integration target of the route (for example <c>integrations/abc123</c>), or <see langword="null"/> when none is attached.</param>
/// <param name="AuthorizationType">The authorization type of the route (for example <c>NONE</c> or <c>JWT</c>), or <see langword="null"/> when not reported.</param>
public sealed record HttpRouteSummary(
    string RouteId,
    string RouteKey,
    string? Target,
    string? AuthorizationType);
