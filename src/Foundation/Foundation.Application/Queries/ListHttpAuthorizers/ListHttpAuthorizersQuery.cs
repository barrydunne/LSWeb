using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.ApiGatewayV2;

namespace Foundation.Application.Queries.ListHttpAuthorizers;

/// <summary>
/// Lists the authorizers of an Amazon API Gateway v2 API.
/// </summary>
/// <param name="ApiId">The unique identifier of the API whose authorizers to list.</param>
public record ListHttpAuthorizersQuery(string ApiId) : IQuery<ListHttpAuthorizersQueryResult>;

/// <summary>
/// The result of listing authorizers.
/// </summary>
/// <param name="Authorizers">The authorizers found on the API.</param>
public record ListHttpAuthorizersQueryResult(IReadOnlyList<HttpAuthorizerSummary> Authorizers);
