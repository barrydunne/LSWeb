using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateHttpApi;
using Foundation.Application.Commands.CreateHttpAuthorizer;
using Foundation.Application.Commands.CreateHttpIntegration;
using Foundation.Application.Commands.CreateHttpRoute;
using Foundation.Application.Commands.CreateHttpStage;
using Foundation.Application.Commands.DeleteHttpApi;
using Foundation.Application.Commands.DeleteHttpAuthorizer;
using Foundation.Application.Commands.DeleteHttpIntegration;
using Foundation.Application.Commands.DeleteHttpRoute;
using Foundation.Application.Commands.DeleteHttpStage;
using Foundation.Application.Commands.TestHttpRoute;
using Foundation.Application.Commands.UpdateHttpApi;
using Foundation.Application.Commands.UpdateHttpAuthorizer;
using Foundation.Application.Commands.UpdateHttpIntegration;
using Foundation.Application.Commands.UpdateHttpRoute;
using Foundation.Application.Commands.UpdateHttpStage;
using Foundation.Application.Queries.GetHttpApi;
using Foundation.Application.Queries.GetHttpAuthorizer;
using Foundation.Application.Queries.GetHttpRoute;
using Foundation.Application.Queries.GetHttpStage;
using Foundation.Application.Queries.ListHttpApis;
using Foundation.Application.Queries.ListHttpAuthorizers;
using Foundation.Application.Queries.ListHttpIntegrations;
using Foundation.Application.Queries.ListHttpRoutes;
using Foundation.Application.Queries.ListHttpStages;
using Foundation.Domain.ApiGatewayV2;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Api.Controllers;

/// <summary>
/// Provides access to Amazon API Gateway v2 APIs: listing the available APIs, viewing the details
/// of a single API, and creating, updating, or deleting APIs.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/services/apigatewayv2")]
public partial class ApiGatewayV2Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiGatewayV2Controller"/> class.
    /// </summary>
    /// <param name="sender">The sender used to dispatch queries and commands.</param>
    /// <param name="logger">The logger.</param>
    public ApiGatewayV2Controller(ISender sender, ILogger<ApiGatewayV2Controller> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Lists the Amazon API Gateway v2 APIs available on the backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the API summaries.</returns>
    [HttpGet("apis")]
    [ProducesResponseType(typeof(HttpApiListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListApis(CancellationToken cancellationToken)
    {
        LogHandlingListApis();
        var result = await _sender.Send(new ListHttpApisQuery(), cancellationToken);
        LogListApisHandled(result.IsSuccess);
        return result.Match(
            apis => Results.Ok(new HttpApiListResponse(
                apis.Apis
                    .Select(api => new HttpApiSummaryResponse(
                        api.ApiId,
                        api.Name,
                        api.ProtocolType,
                        api.ApiEndpoint,
                        api.CreatedDate))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Gets the details of a single Amazon API Gateway v2 API by its identifier.
    /// </summary>
    /// <param name="apiId">The unique identifier of the API to read.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the API details.</returns>
    [HttpGet("apis/{apiId}")]
    [ProducesResponseType(typeof(HttpApiDetailResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetApi(string apiId, CancellationToken cancellationToken)
    {
        LogHandlingGetApi(apiId);
        var result = await _sender.Send(new GetHttpApiQuery(apiId), cancellationToken);
        LogGetApiHandled(result.IsSuccess);
        return result.Match(
            api => Results.Ok(ToDetailResponse(api.Api)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a new Amazon API Gateway v2 API.
    /// </summary>
    /// <param name="request">The API configuration to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result carrying the new API id.</returns>
    [HttpPost("apis")]
    [ProducesResponseType(typeof(HttpApiCreatedResponse), StatusCodes.Status201Created)]
    public async Task<IResult> CreateApi(
        [FromBody] HttpApiCreateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreateApi(request.Name);
        var result = await _sender.Send(
            new CreateHttpApiCommand(
                request.Name,
                request.ProtocolType,
                request.Description,
                request.Version,
                request.RouteSelectionExpression),
            cancellationToken);
        LogCreateApiHandled(result.IsSuccess);
        return result.Match(
            apiId => Results.Created(
                $"/api/services/apigatewayv2/apis/{Uri.EscapeDataString(apiId)}",
                new HttpApiCreatedResponse(apiId)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Updates the configuration of an existing Amazon API Gateway v2 API.
    /// </summary>
    /// <param name="apiId">The unique identifier of the API to update.</param>
    /// <param name="request">The API configuration to apply.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("apis/{apiId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> UpdateApi(
        string apiId, [FromBody] HttpApiUpdateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingUpdateApi(apiId);
        var result = await _sender.Send(
            new UpdateHttpApiCommand(
                apiId,
                request.Name,
                request.ProtocolType,
                request.Description,
                request.Version,
                request.RouteSelectionExpression,
                ToCorsConfiguration(request.CorsConfiguration)),
            cancellationToken);
        LogUpdateApiHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    private static HttpApiCorsConfiguration? ToCorsConfiguration(HttpApiCorsRequest? cors)
        => cors is null
            ? null
            : new HttpApiCorsConfiguration(
                cors.AllowCredentials,
                cors.AllowHeaders,
                cors.AllowMethods,
                cors.AllowOrigins,
                cors.ExposeHeaders,
                cors.MaxAge);

    /// <summary>
    /// Deletes an Amazon API Gateway v2 API by its identifier. This action cannot be undone.
    /// </summary>
    /// <param name="apiId">The unique identifier of the API to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("apis/{apiId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteApi(string apiId, CancellationToken cancellationToken)
    {
        LogHandlingDeleteApi(apiId);
        var result = await _sender.Send(new DeleteHttpApiCommand(apiId), cancellationToken);
        LogDeleteApiHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Invokes a route of an Amazon API Gateway v2 API to verify its authorization behaviour.
    /// </summary>
    /// <remarks>
    /// The route is invoked over HTTP with and without a bearer token so the caller can observe
    /// whether unauthenticated requests are rejected and authenticated requests are forwarded to the
    /// integration, all without using external command-line tooling.
    /// </remarks>
    /// <param name="apiId">The identifier of the API the route belongs to.</param>
    /// <param name="request">The invocation request describing the stage, method, path and token.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the invocation outcome.</returns>
    [HttpPost("apis/{apiId}/routes/test-invoke")]
    [ProducesResponseType(typeof(HttpRouteInvocationResponse), StatusCodes.Status200OK)]
    public async Task<IResult> TestInvokeRoute(
        string apiId,
        [FromBody] HttpRouteTestRequest request,
        CancellationToken cancellationToken)
    {
        LogHandlingTestInvokeRoute(request.Method, request.Path, apiId);
        var result = await _sender.Send(
            new TestHttpRouteCommand(
                apiId,
                request.Stage,
                request.Method,
                request.Path,
                request.Token,
                request.Body),
            cancellationToken);
        LogTestInvokeRouteHandled(result.IsSuccess);
        return result.Match(
            invocation => Results.Ok(new HttpRouteInvocationResponse(
                invocation.StatusCode,
                invocation.Authorized,
                invocation.LatencyMilliseconds,
                invocation.Headers,
                invocation.Body)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the routes of an Amazon API Gateway v2 API.
    /// </summary>
    /// <param name="apiId">The identifier of the API whose routes to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the route summaries.</returns>
    [HttpGet("apis/{apiId}/routes")]
    [ProducesResponseType(typeof(HttpRouteListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListRoutes(string apiId, CancellationToken cancellationToken)
    {
        LogHandlingListRoutes(apiId);
        var result = await _sender.Send(new ListHttpRoutesQuery(apiId), cancellationToken);
        LogListRoutesHandled(result.IsSuccess);
        return result.Match(
            routes => Results.Ok(new HttpRouteListResponse(
                routes.Routes
                    .Select(route => new HttpRouteSummaryResponse(
                        route.RouteId,
                        route.RouteKey,
                        route.Target,
                        route.AuthorizationType))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Gets the details of a single Amazon API Gateway v2 route by its identifier.
    /// </summary>
    /// <param name="apiId">The identifier of the API the route belongs to.</param>
    /// <param name="routeId">The unique identifier of the route to read.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the route details.</returns>
    [HttpGet("apis/{apiId}/routes/{routeId}")]
    [ProducesResponseType(typeof(HttpRouteDetailResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetRoute(string apiId, string routeId, CancellationToken cancellationToken)
    {
        LogHandlingGetRoute(routeId, apiId);
        var result = await _sender.Send(new GetHttpRouteQuery(apiId, routeId), cancellationToken);
        LogGetRouteHandled(result.IsSuccess);
        return result.Match(
            route => Results.Ok(ToRouteDetailResponse(route.Route)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a new route on an Amazon API Gateway v2 API.
    /// </summary>
    /// <param name="apiId">The identifier of the API to add the route to.</param>
    /// <param name="request">The route configuration to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result carrying the new route id.</returns>
    [HttpPost("apis/{apiId}/routes")]
    [ProducesResponseType(typeof(HttpRouteCreatedResponse), StatusCodes.Status201Created)]
    public async Task<IResult> CreateRoute(
        string apiId, [FromBody] HttpRouteCreateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreateRoute(request.RouteKey, apiId);
        var result = await _sender.Send(
            new CreateHttpRouteCommand(
                apiId,
                request.RouteKey,
                request.Target,
                request.AuthorizationType,
                request.AuthorizerId,
                request.AuthorizationScopes ?? []),
            cancellationToken);
        LogCreateRouteHandled(result.IsSuccess);
        return result.Match(
            routeId => Results.Created(
                $"/api/services/apigatewayv2/apis/{Uri.EscapeDataString(apiId)}/routes/{Uri.EscapeDataString(routeId)}",
                new HttpRouteCreatedResponse(routeId)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Updates the configuration of an existing Amazon API Gateway v2 route.
    /// </summary>
    /// <param name="apiId">The identifier of the API the route belongs to.</param>
    /// <param name="routeId">The unique identifier of the route to update.</param>
    /// <param name="request">The route configuration to apply.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("apis/{apiId}/routes/{routeId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> UpdateRoute(
        string apiId, string routeId, [FromBody] HttpRouteUpdateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingUpdateRoute(routeId, apiId);
        var result = await _sender.Send(
            new UpdateHttpRouteCommand(
                apiId,
                routeId,
                request.RouteKey,
                request.Target,
                request.AuthorizationType,
                request.AuthorizerId,
                request.AuthorizationScopes ?? []),
            cancellationToken);
        LogUpdateRouteHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes a route from an Amazon API Gateway v2 API. This action cannot be undone.
    /// </summary>
    /// <param name="apiId">The identifier of the API the route belongs to.</param>
    /// <param name="routeId">The unique identifier of the route to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("apis/{apiId}/routes/{routeId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteRoute(string apiId, string routeId, CancellationToken cancellationToken)
    {
        LogHandlingDeleteRoute(routeId, apiId);
        var result = await _sender.Send(new DeleteHttpRouteCommand(apiId, routeId), cancellationToken);
        LogDeleteRouteHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the integrations of an Amazon API Gateway v2 API.
    /// </summary>
    /// <param name="apiId">The identifier of the API whose integrations to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the integration summaries.</returns>
    [HttpGet("apis/{apiId}/integrations")]
    [ProducesResponseType(typeof(HttpIntegrationListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListIntegrations(string apiId, CancellationToken cancellationToken)
    {
        LogHandlingListIntegrations(apiId);
        var result = await _sender.Send(new ListHttpIntegrationsQuery(apiId), cancellationToken);
        LogListIntegrationsHandled(result.IsSuccess);
        return result.Match(
            integrations => Results.Ok(new HttpIntegrationListResponse(
                integrations.Integrations
                    .Select(integration => new HttpIntegrationSummaryResponse(
                        integration.IntegrationId,
                        integration.IntegrationType,
                        integration.IntegrationMethod,
                        integration.IntegrationUri,
                        integration.PayloadFormatVersion,
                        integration.Description))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a new integration on an Amazon API Gateway v2 API.
    /// </summary>
    /// <param name="apiId">The identifier of the API to add the integration to.</param>
    /// <param name="request">The integration configuration to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result carrying the new integration id.</returns>
    [HttpPost("apis/{apiId}/integrations")]
    [ProducesResponseType(typeof(HttpIntegrationCreatedResponse), StatusCodes.Status201Created)]
    public async Task<IResult> CreateIntegration(
        string apiId, [FromBody] HttpIntegrationCreateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreateIntegration(request.IntegrationType, apiId);
        var result = await _sender.Send(
            new CreateHttpIntegrationCommand(
                apiId,
                request.IntegrationType,
                request.IntegrationMethod,
                request.IntegrationUri,
                request.PayloadFormatVersion,
                request.Description),
            cancellationToken);
        LogCreateIntegrationHandled(result.IsSuccess);
        return result.Match(
            integrationId => Results.Created(
                $"/api/services/apigatewayv2/apis/{Uri.EscapeDataString(apiId)}/integrations/{Uri.EscapeDataString(integrationId)}",
                new HttpIntegrationCreatedResponse(integrationId)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Updates an existing integration on an Amazon API Gateway v2 API.
    /// </summary>
    /// <param name="apiId">The identifier of the API the integration belongs to.</param>
    /// <param name="integrationId">The unique identifier of the integration to update.</param>
    /// <param name="request">The integration configuration to apply.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("apis/{apiId}/integrations/{integrationId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> UpdateIntegration(
        string apiId, string integrationId, [FromBody] HttpIntegrationUpdateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingUpdateIntegration(integrationId, apiId);
        var result = await _sender.Send(
            new UpdateHttpIntegrationCommand(
                apiId,
                integrationId,
                request.IntegrationType,
                request.IntegrationMethod,
                request.IntegrationUri,
                request.PayloadFormatVersion,
                request.Description),
            cancellationToken);
        LogUpdateIntegrationHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes an integration from an Amazon API Gateway v2 API. This action cannot be undone.
    /// </summary>
    /// <param name="apiId">The identifier of the API the integration belongs to.</param>
    /// <param name="integrationId">The unique identifier of the integration to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("apis/{apiId}/integrations/{integrationId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteIntegration(string apiId, string integrationId, CancellationToken cancellationToken)
    {
        LogHandlingDeleteIntegration(integrationId, apiId);
        var result = await _sender.Send(new DeleteHttpIntegrationCommand(apiId, integrationId), cancellationToken);
        LogDeleteIntegrationHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the authorizers of an Amazon API Gateway v2 API.
    /// </summary>
    /// <param name="apiId">The identifier of the API whose authorizers to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the authorizer summaries.</returns>
    [HttpGet("apis/{apiId}/authorizers")]
    [ProducesResponseType(typeof(HttpAuthorizerListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListAuthorizers(string apiId, CancellationToken cancellationToken)
    {
        LogHandlingListAuthorizers(apiId);
        var result = await _sender.Send(new ListHttpAuthorizersQuery(apiId), cancellationToken);
        LogListAuthorizersHandled(result.IsSuccess);
        return result.Match(
            authorizers => Results.Ok(new HttpAuthorizerListResponse(
                authorizers.Authorizers
                    .Select(authorizer => new HttpAuthorizerSummaryResponse(
                        authorizer.AuthorizerId,
                        authorizer.Name,
                        authorizer.AuthorizerType))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Gets the details of a single Amazon API Gateway v2 authorizer.
    /// </summary>
    /// <param name="apiId">The identifier of the API the authorizer belongs to.</param>
    /// <param name="authorizerId">The identifier of the authorizer to read.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the authorizer detail.</returns>
    [HttpGet("apis/{apiId}/authorizers/{authorizerId}")]
    [ProducesResponseType(typeof(HttpAuthorizerDetailResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetAuthorizer(string apiId, string authorizerId, CancellationToken cancellationToken)
    {
        LogHandlingGetAuthorizer(authorizerId, apiId);
        var result = await _sender.Send(new GetHttpAuthorizerQuery(apiId, authorizerId), cancellationToken);
        LogGetAuthorizerHandled(result.IsSuccess);
        return result.Match(
            authorizer => Results.Ok(ToAuthorizerDetailResponse(authorizer.Authorizer)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a new authorizer on an Amazon API Gateway v2 API.
    /// </summary>
    /// <param name="apiId">The identifier of the API to add the authorizer to.</param>
    /// <param name="request">The authorizer configuration to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result carrying the new authorizer id.</returns>
    [HttpPost("apis/{apiId}/authorizers")]
    [ProducesResponseType(typeof(HttpAuthorizerCreatedResponse), StatusCodes.Status201Created)]
    public async Task<IResult> CreateAuthorizer(
        string apiId, [FromBody] HttpAuthorizerCreateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreateAuthorizer(request.Name, apiId);
        var result = await _sender.Send(
            new CreateHttpAuthorizerCommand(
                apiId,
                request.Name,
                request.AuthorizerType,
                request.IdentitySource,
                request.JwtIssuer,
                request.JwtAudience),
            cancellationToken);
        LogCreateAuthorizerHandled(result.IsSuccess);
        return result.Match(
            authorizerId => Results.Created(
                $"/api/services/apigatewayv2/apis/{Uri.EscapeDataString(apiId)}/authorizers/{Uri.EscapeDataString(authorizerId)}",
                new HttpAuthorizerCreatedResponse(authorizerId)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Updates an existing authorizer on an Amazon API Gateway v2 API.
    /// </summary>
    /// <param name="apiId">The identifier of the API the authorizer belongs to.</param>
    /// <param name="authorizerId">The identifier of the authorizer to update.</param>
    /// <param name="request">The authorizer configuration to apply.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("apis/{apiId}/authorizers/{authorizerId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> UpdateAuthorizer(
        string apiId, string authorizerId, [FromBody] HttpAuthorizerUpdateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingUpdateAuthorizer(authorizerId, apiId);
        var result = await _sender.Send(
            new UpdateHttpAuthorizerCommand(
                apiId,
                authorizerId,
                request.Name,
                request.AuthorizerType,
                request.IdentitySource,
                request.JwtIssuer,
                request.JwtAudience),
            cancellationToken);
        LogUpdateAuthorizerHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes an authorizer from an Amazon API Gateway v2 API.
    /// </summary>
    /// <param name="apiId">The identifier of the API the authorizer belongs to.</param>
    /// <param name="authorizerId">The identifier of the authorizer to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("apis/{apiId}/authorizers/{authorizerId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteAuthorizer(string apiId, string authorizerId, CancellationToken cancellationToken)
    {
        LogHandlingDeleteAuthorizer(authorizerId, apiId);
        var result = await _sender.Send(new DeleteHttpAuthorizerCommand(apiId, authorizerId), cancellationToken);
        LogDeleteAuthorizerHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the stages of an Amazon API Gateway v2 API.
    /// </summary>
    /// <param name="apiId">The identifier of the API whose stages to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the stage summaries.</returns>
    [HttpGet("apis/{apiId}/stages")]
    [ProducesResponseType(typeof(HttpStageListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListStages(string apiId, CancellationToken cancellationToken)
    {
        LogHandlingListStages(apiId);
        var result = await _sender.Send(new ListHttpStagesQuery(apiId), cancellationToken);
        LogListStagesHandled(result.IsSuccess);
        return result.Match(
            stages => Results.Ok(new HttpStageListResponse(
                stages.Stages
                    .Select(stage => new HttpStageSummaryResponse(
                        stage.StageName,
                        stage.AutoDeploy,
                        stage.DeploymentId,
                        stage.CreatedDate))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Gets the details of a single Amazon API Gateway v2 stage.
    /// </summary>
    /// <param name="apiId">The identifier of the API the stage belongs to.</param>
    /// <param name="stageName">The name of the stage to read.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the stage detail.</returns>
    [HttpGet("apis/{apiId}/stages/{stageName}")]
    [ProducesResponseType(typeof(HttpStageDetailResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetStage(string apiId, string stageName, CancellationToken cancellationToken)
    {
        LogHandlingGetStage(stageName, apiId);
        var result = await _sender.Send(new GetHttpStageQuery(apiId, stageName), cancellationToken);
        LogGetStageHandled(result.IsSuccess);
        return result.Match(
            stage => Results.Ok(ToStageDetailResponse(stage.Stage)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a new stage on an Amazon API Gateway v2 API.
    /// </summary>
    /// <param name="apiId">The identifier of the API to add the stage to.</param>
    /// <param name="request">The stage configuration to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result carrying the new stage name.</returns>
    [HttpPost("apis/{apiId}/stages")]
    [ProducesResponseType(typeof(HttpStageCreatedResponse), StatusCodes.Status201Created)]
    public async Task<IResult> CreateStage(
        string apiId, [FromBody] HttpStageCreateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreateStage(request.StageName, apiId);
        var result = await _sender.Send(
            new CreateHttpStageCommand(
                apiId,
                request.StageName,
                request.AutoDeploy,
                request.Description,
                request.DefaultRouteThrottlingBurstLimit,
                request.DefaultRouteThrottlingRateLimit,
                request.StageVariables),
            cancellationToken);
        LogCreateStageHandled(result.IsSuccess);
        return result.Match(
            stageName => Results.Created(
                $"/api/services/apigatewayv2/apis/{Uri.EscapeDataString(apiId)}/stages/{Uri.EscapeDataString(stageName)}",
                new HttpStageCreatedResponse(stageName)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Updates an existing stage on an Amazon API Gateway v2 API.
    /// </summary>
    /// <param name="apiId">The identifier of the API the stage belongs to.</param>
    /// <param name="stageName">The name of the stage to update.</param>
    /// <param name="request">The stage configuration to apply.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("apis/{apiId}/stages/{stageName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> UpdateStage(
        string apiId, string stageName, [FromBody] HttpStageUpdateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingUpdateStage(stageName, apiId);
        var result = await _sender.Send(
            new UpdateHttpStageCommand(
                apiId,
                stageName,
                request.AutoDeploy,
                request.Description,
                request.DefaultRouteThrottlingBurstLimit,
                request.DefaultRouteThrottlingRateLimit,
                request.StageVariables),
            cancellationToken);
        LogUpdateStageHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes a stage from an Amazon API Gateway v2 API.
    /// </summary>
    /// <param name="apiId">The identifier of the API the stage belongs to.</param>
    /// <param name="stageName">The name of the stage to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("apis/{apiId}/stages/{stageName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteStage(string apiId, string stageName, CancellationToken cancellationToken)
    {
        LogHandlingDeleteStage(stageName, apiId);
        var result = await _sender.Send(new DeleteHttpStageCommand(apiId, stageName), cancellationToken);
        LogDeleteStageHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    private static HttpApiDetailResponse ToDetailResponse(Foundation.Domain.ApiGatewayV2.HttpApiDetail api)
        => new(
            api.ApiId,
            api.Name,
            api.ProtocolType,
            api.ApiEndpoint,
            api.Description,
            api.Version,
            api.RouteSelectionExpression,
            api.CorsConfiguration is null
                ? null
                : new HttpApiCorsResponse(
                    api.CorsConfiguration.AllowCredentials,
                    api.CorsConfiguration.AllowHeaders,
                    api.CorsConfiguration.AllowMethods,
                    api.CorsConfiguration.AllowOrigins,
                    api.CorsConfiguration.ExposeHeaders,
                    api.CorsConfiguration.MaxAge),
            api.CreatedDate);

    private static HttpRouteDetailResponse ToRouteDetailResponse(Foundation.Domain.ApiGatewayV2.HttpRouteDetail route)
        => new(
            route.RouteId,
            route.RouteKey,
            route.Target,
            route.AuthorizationType,
            route.AuthorizerId,
            route.AuthorizationScopes,
            route.ApiKeyRequired);

    private static HttpAuthorizerDetailResponse ToAuthorizerDetailResponse(Foundation.Domain.ApiGatewayV2.HttpAuthorizerDetail authorizer)
        => new(
            authorizer.AuthorizerId,
            authorizer.Name,
            authorizer.AuthorizerType,
            authorizer.IdentitySource,
            authorizer.JwtIssuer,
            authorizer.JwtAudience);

    private static HttpStageDetailResponse ToStageDetailResponse(Foundation.Domain.ApiGatewayV2.HttpStageDetail stage)
        => new(
            stage.StageName,
            stage.AutoDeploy,
            stage.DeploymentId,
            stage.Description,
            stage.DefaultRouteThrottlingBurstLimit,
            stage.DefaultRouteThrottlingRateLimit,
            stage.StageVariables,
            stage.CreatedDate,
            stage.LastUpdatedDate);

    [LoggerMessage(LogLevel.Trace, "Handling API Gateway v2 API list request.")]
    private partial void LogHandlingListApis();

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 API list request handled. Success: {Success}")]
    private partial void LogListApisHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling API Gateway v2 API get request for {ApiId}.")]
    private partial void LogHandlingGetApi(string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 API get request handled. Success: {Success}")]
    private partial void LogGetApiHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling API Gateway v2 API create request for {Name}.")]
    private partial void LogHandlingCreateApi(string name);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 API create request handled. Success: {Success}")]
    private partial void LogCreateApiHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling API Gateway v2 API update request for {ApiId}.")]
    private partial void LogHandlingUpdateApi(string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 API update request handled. Success: {Success}")]
    private partial void LogUpdateApiHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling API Gateway v2 API delete request for {ApiId}.")]
    private partial void LogHandlingDeleteApi(string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 API delete request handled. Success: {Success}")]
    private partial void LogDeleteApiHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling API Gateway v2 route test invoke request for {Method} {Path} on {ApiId}.")]
    private partial void LogHandlingTestInvokeRoute(string method, string path, string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 route test invoke request handled. Success: {Success}")]
    private partial void LogTestInvokeRouteHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling API Gateway v2 route list request for {ApiId}.")]
    private partial void LogHandlingListRoutes(string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 route list request handled. Success: {Success}")]
    private partial void LogListRoutesHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling API Gateway v2 route get request for {RouteId} in {ApiId}.")]
    private partial void LogHandlingGetRoute(string routeId, string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 route get request handled. Success: {Success}")]
    private partial void LogGetRouteHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling API Gateway v2 route create request for {RouteKey} in {ApiId}.")]
    private partial void LogHandlingCreateRoute(string routeKey, string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 route create request handled. Success: {Success}")]
    private partial void LogCreateRouteHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling API Gateway v2 route update request for {RouteId} in {ApiId}.")]
    private partial void LogHandlingUpdateRoute(string routeId, string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 route update request handled. Success: {Success}")]
    private partial void LogUpdateRouteHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling API Gateway v2 route delete request for {RouteId} in {ApiId}.")]
    private partial void LogHandlingDeleteRoute(string routeId, string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 route delete request handled. Success: {Success}")]
    private partial void LogDeleteRouteHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling API Gateway v2 integration list request for {ApiId}.")]
    private partial void LogHandlingListIntegrations(string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 integration list request handled. Success: {Success}")]
    private partial void LogListIntegrationsHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling API Gateway v2 integration create request for {IntegrationType} in {ApiId}.")]
    private partial void LogHandlingCreateIntegration(string integrationType, string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 integration create request handled. Success: {Success}")]
    private partial void LogCreateIntegrationHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling API Gateway v2 integration update request for {IntegrationId} in {ApiId}.")]
    private partial void LogHandlingUpdateIntegration(string integrationId, string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 integration update request handled. Success: {Success}")]
    private partial void LogUpdateIntegrationHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling API Gateway v2 integration delete request for {IntegrationId} in {ApiId}.")]
    private partial void LogHandlingDeleteIntegration(string integrationId, string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 integration delete request handled. Success: {Success}")]
    private partial void LogDeleteIntegrationHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling API Gateway v2 authorizer list request for {ApiId}.")]
    private partial void LogHandlingListAuthorizers(string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 authorizer list request handled. Success: {Success}")]
    private partial void LogListAuthorizersHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling API Gateway v2 authorizer get request for {AuthorizerId} in {ApiId}.")]
    private partial void LogHandlingGetAuthorizer(string authorizerId, string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 authorizer get request handled. Success: {Success}")]
    private partial void LogGetAuthorizerHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling API Gateway v2 authorizer create request for {Name} in {ApiId}.")]
    private partial void LogHandlingCreateAuthorizer(string name, string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 authorizer create request handled. Success: {Success}")]
    private partial void LogCreateAuthorizerHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling API Gateway v2 authorizer update request for {AuthorizerId} in {ApiId}.")]
    private partial void LogHandlingUpdateAuthorizer(string authorizerId, string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 authorizer update request handled. Success: {Success}")]
    private partial void LogUpdateAuthorizerHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling API Gateway v2 authorizer delete request for {AuthorizerId} in {ApiId}.")]
    private partial void LogHandlingDeleteAuthorizer(string authorizerId, string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 authorizer delete request handled. Success: {Success}")]
    private partial void LogDeleteAuthorizerHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling API Gateway v2 stage list request for {ApiId}.")]
    private partial void LogHandlingListStages(string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 stage list request handled. Success: {Success}")]
    private partial void LogListStagesHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling API Gateway v2 stage get request for {StageName} in {ApiId}.")]
    private partial void LogHandlingGetStage(string stageName, string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 stage get request handled. Success: {Success}")]
    private partial void LogGetStageHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling API Gateway v2 stage create request for {StageName} in {ApiId}.")]
    private partial void LogHandlingCreateStage(string stageName, string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 stage create request handled. Success: {Success}")]
    private partial void LogCreateStageHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling API Gateway v2 stage update request for {StageName} in {ApiId}.")]
    private partial void LogHandlingUpdateStage(string stageName, string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 stage update request handled. Success: {Success}")]
    private partial void LogUpdateStageHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling API Gateway v2 stage delete request for {StageName} in {ApiId}.")]
    private partial void LogHandlingDeleteStage(string stageName, string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 stage delete request handled. Success: {Success}")]
    private partial void LogDeleteStageHandled(bool success);
}
