using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.ApiGatewayV2;

namespace Foundation.Application.Queries.ListHttpRoutes;

/// <summary>
/// Lists the routes of an Amazon API Gateway v2 API.
/// </summary>
/// <param name="ApiId">The unique identifier of the API whose routes to list.</param>
public record ListHttpRoutesQuery(string ApiId) : IQuery<ListHttpRoutesQueryResult>;

/// <summary>
/// The result of listing routes.
/// </summary>
/// <param name="Routes">The routes found on the API.</param>
public record ListHttpRoutesQueryResult(IReadOnlyList<HttpRouteSummary> Routes);
