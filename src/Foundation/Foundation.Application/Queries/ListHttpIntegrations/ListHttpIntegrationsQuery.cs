using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.ApiGatewayV2;

namespace Foundation.Application.Queries.ListHttpIntegrations;

/// <summary>
/// Lists the integrations of an Amazon API Gateway v2 API. An integration is the backend target a
/// route forwards requests to.
/// </summary>
/// <param name="ApiId">The unique identifier of the API whose integrations to list.</param>
public record ListHttpIntegrationsQuery(string ApiId) : IQuery<ListHttpIntegrationsQueryResult>;

/// <summary>
/// The result of listing integrations.
/// </summary>
/// <param name="Integrations">The integrations found on the API.</param>
public record ListHttpIntegrationsQueryResult(IReadOnlyList<HttpIntegrationSummary> Integrations);
