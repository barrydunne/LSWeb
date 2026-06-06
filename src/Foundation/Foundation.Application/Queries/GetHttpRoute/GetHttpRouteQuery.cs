using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.ApiGatewayV2;

namespace Foundation.Application.Queries.GetHttpRoute;

/// <summary>
/// Reads the full configuration of a single Amazon API Gateway v2 route.
/// </summary>
/// <param name="ApiId">The unique identifier of the API the route belongs to.</param>
/// <param name="RouteId">The unique identifier of the route to read.</param>
public record GetHttpRouteQuery(string ApiId, string RouteId) : IQuery<GetHttpRouteQueryResult>;

/// <summary>
/// The result of reading a route.
/// </summary>
/// <param name="Route">The route detail.</param>
public record GetHttpRouteQueryResult(HttpRouteDetail Route);
