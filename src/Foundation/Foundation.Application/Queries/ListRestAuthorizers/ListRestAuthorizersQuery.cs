using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.ApiGateway;

namespace Foundation.Application.Queries.ListRestAuthorizers;

/// <summary>
/// List the authorizers configured on an API Gateway REST API.
/// </summary>
/// <param name="RestApiId">The identifier of the REST API whose authorizers to list.</param>
public record ListRestAuthorizersQuery(string RestApiId) : IQuery<ListRestAuthorizersQueryResult>;

/// <summary>
/// The result of listing the authorizers of a REST API.
/// </summary>
/// <param name="Authorizers">The authorizers of the REST API.</param>
public record ListRestAuthorizersQueryResult(IReadOnlyList<RestAuthorizerSummary> Authorizers);
