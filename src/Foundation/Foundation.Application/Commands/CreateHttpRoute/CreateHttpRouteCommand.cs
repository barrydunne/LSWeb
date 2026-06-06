using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.CreateHttpRoute;

/// <summary>
/// Create an Amazon API Gateway v2 route from the supplied configuration.
/// </summary>
/// <param name="ApiId">The identifier of the API the route belongs to.</param>
/// <param name="RouteKey">The route key (for example <c>GET /items</c> or <c>$default</c>).</param>
/// <param name="Target">The integration target of the route (for example <c>integrations/abc123</c>), or <see langword="null"/> for none.</param>
/// <param name="AuthorizationType">The authorization type of the route (for example <c>NONE</c> or <c>JWT</c>), or <see langword="null"/> to use the backend default.</param>
/// <param name="AuthorizerId">The identifier of the authorizer to attach to the route, or <see langword="null"/> for none.</param>
/// <param name="AuthorizationScopes">The authorization scopes required by the route.</param>
public record CreateHttpRouteCommand(
    string ApiId,
    string RouteKey,
    string? Target,
    string? AuthorizationType,
    string? AuthorizerId,
    IReadOnlyList<string> AuthorizationScopes) : ICommand<string>;
