using AspNet.KickStarter.FunctionalResult;
using Foundation.Domain.ApiGatewayV2;

namespace Foundation.Application.ApiGatewayV2;

/// <summary>
/// Provides access to Amazon API Gateway v2 APIs (HTTP and WebSocket) on the configured backend.
/// </summary>
public interface IApiGatewayV2Client
{
    /// <summary>
    /// Lists the APIs available on the backend.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The APIs, or an error if the backend could not be reached.</returns>
    Task<Result<IReadOnlyList<HttpApiSummary>>> ListApisAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Reads the full configuration of a single API.
    /// </summary>
    /// <param name="apiId">The unique identifier of the API.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The API detail, or an error if the API could not be read.</returns>
    Task<Result<HttpApiDetail>> GetApiAsync(string apiId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new API from the supplied specification.
    /// </summary>
    /// <param name="specification">The desired configuration of the API to create.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The identifier of the created API, or an error if it could not be created.</returns>
    Task<Result<string>> CreateApiAsync(HttpApiSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the configuration of an existing API.
    /// </summary>
    /// <param name="specification">The desired configuration of the API to update.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A success result, or an error if the API could not be updated.</returns>
    Task<Result> UpdateApiAsync(HttpApiSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an API by its identifier. This action cannot be undone.
    /// </summary>
    /// <param name="apiId">The unique identifier of the API to delete.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A success result, or an error if the API could not be deleted.</returns>
    Task<Result> DeleteApiAsync(string apiId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists the routes of an API.
    /// </summary>
    /// <param name="apiId">The unique identifier of the API.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The routes, or an error if they could not be read.</returns>
    Task<Result<IReadOnlyList<HttpRouteSummary>>> ListRoutesAsync(string apiId, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the full configuration of a single route.
    /// </summary>
    /// <param name="apiId">The unique identifier of the API.</param>
    /// <param name="routeId">The unique identifier of the route.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The route detail, or an error if the route could not be read.</returns>
    Task<Result<HttpRouteDetail>> GetRouteAsync(string apiId, string routeId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new route from the supplied specification.
    /// </summary>
    /// <param name="specification">The desired configuration of the route to create.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The identifier of the created route, or an error if it could not be created.</returns>
    Task<Result<string>> CreateRouteAsync(HttpRouteSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the configuration of an existing route.
    /// </summary>
    /// <param name="specification">The desired configuration of the route to update.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A success result, or an error if the route could not be updated.</returns>
    Task<Result> UpdateRouteAsync(HttpRouteSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a route by its identifier. This action cannot be undone.
    /// </summary>
    /// <param name="apiId">The unique identifier of the API.</param>
    /// <param name="routeId">The unique identifier of the route to delete.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A success result, or an error if the route could not be deleted.</returns>
    Task<Result> DeleteRouteAsync(string apiId, string routeId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists the integrations of an API. An integration is the backend target a route forwards requests to.
    /// </summary>
    /// <param name="apiId">The unique identifier of the API.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The integrations, or an error if they could not be read.</returns>
    Task<Result<IReadOnlyList<HttpIntegrationSummary>>> ListIntegrationsAsync(string apiId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new integration from the supplied specification.
    /// </summary>
    /// <param name="specification">The desired configuration of the integration to create.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The identifier of the created integration, or an error if it could not be created.</returns>
    Task<Result<string>> CreateIntegrationAsync(HttpIntegrationSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing integration with the supplied specification.
    /// </summary>
    /// <param name="specification">The desired configuration of the integration, including its identifier.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A successful result, or an error if the integration could not be updated.</returns>
    Task<Result> UpdateIntegrationAsync(HttpIntegrationSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an integration of an API. This action cannot be undone.
    /// </summary>
    /// <param name="apiId">The unique identifier of the API.</param>
    /// <param name="integrationId">The unique identifier of the integration to delete.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A successful result, or an error if the integration could not be deleted.</returns>
    Task<Result> DeleteIntegrationAsync(string apiId, string integrationId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists the authorizers of an API.
    /// </summary>
    /// <param name="apiId">The unique identifier of the API.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The authorizers, or an error if they could not be read.</returns>
    Task<Result<IReadOnlyList<HttpAuthorizerSummary>>> ListAuthorizersAsync(string apiId, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the full configuration of a single authorizer.
    /// </summary>
    /// <param name="apiId">The unique identifier of the API.</param>
    /// <param name="authorizerId">The unique identifier of the authorizer.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The authorizer detail, or an error if the authorizer could not be read.</returns>
    Task<Result<HttpAuthorizerDetail>> GetAuthorizerAsync(string apiId, string authorizerId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new authorizer from the supplied specification.
    /// </summary>
    /// <param name="specification">The desired configuration of the authorizer to create.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The identifier of the created authorizer, or an error if it could not be created.</returns>
    Task<Result<string>> CreateAuthorizerAsync(HttpAuthorizerSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the configuration of an existing authorizer.
    /// </summary>
    /// <param name="specification">The desired configuration of the authorizer to update.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A success result, or an error if the authorizer could not be updated.</returns>
    Task<Result> UpdateAuthorizerAsync(HttpAuthorizerSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an authorizer by its identifier. This action cannot be undone.
    /// </summary>
    /// <param name="apiId">The unique identifier of the API.</param>
    /// <param name="authorizerId">The unique identifier of the authorizer to delete.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A success result, or an error if the authorizer could not be deleted.</returns>
    Task<Result> DeleteAuthorizerAsync(string apiId, string authorizerId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists the stages of an API.
    /// </summary>
    /// <param name="apiId">The unique identifier of the API.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The stages, or an error if they could not be read.</returns>
    Task<Result<IReadOnlyList<HttpStageSummary>>> ListStagesAsync(string apiId, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the full configuration of a single stage.
    /// </summary>
    /// <param name="apiId">The unique identifier of the API.</param>
    /// <param name="stageName">The name of the stage.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The stage detail, or an error if the stage could not be read.</returns>
    Task<Result<HttpStageDetail>> GetStageAsync(string apiId, string stageName, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new stage from the supplied specification.
    /// </summary>
    /// <param name="specification">The desired configuration of the stage to create.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The name of the created stage, or an error if it could not be created.</returns>
    Task<Result<string>> CreateStageAsync(HttpStageSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the configuration of an existing stage.
    /// </summary>
    /// <param name="specification">The desired configuration of the stage to update.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A success result, or an error if the stage could not be updated.</returns>
    Task<Result> UpdateStageAsync(HttpStageSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a stage by its name. This action cannot be undone.
    /// </summary>
    /// <param name="apiId">The unique identifier of the API.</param>
    /// <param name="stageName">The name of the stage to delete.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A success result, or an error if the stage could not be deleted.</returns>
    Task<Result> DeleteStageAsync(string apiId, string stageName, CancellationToken cancellationToken);
}
