using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.ApiGateway;

namespace Foundation.Application.Queries.ListRestDeployments;

/// <summary>
/// List the deployments of an API Gateway REST API.
/// </summary>
/// <param name="RestApiId">The identifier of the REST API whose deployments to list.</param>
public record ListRestDeploymentsQuery(string RestApiId) : IQuery<ListRestDeploymentsQueryResult>;

/// <summary>
/// The result of listing the deployments of a REST API.
/// </summary>
/// <param name="Deployments">The deployments of the REST API.</param>
public record ListRestDeploymentsQueryResult(IReadOnlyList<RestDeploymentSummary> Deployments);
