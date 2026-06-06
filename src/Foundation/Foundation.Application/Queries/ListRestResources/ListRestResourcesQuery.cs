using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.ApiGateway;

namespace Foundation.Application.Queries.ListRestResources;

/// <summary>
/// List the resource tree of an API Gateway REST API.
/// </summary>
/// <param name="RestApiId">The identifier of the REST API whose resources to list.</param>
public record ListRestResourcesQuery(string RestApiId) : IQuery<ListRestResourcesQueryResult>;

/// <summary>
/// The result of listing the resources of a REST API.
/// </summary>
/// <param name="Resources">The resources of the REST API.</param>
public record ListRestResourcesQueryResult(IReadOnlyList<RestResourceSummary> Resources);
