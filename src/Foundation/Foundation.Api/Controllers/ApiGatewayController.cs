using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateRestApi;
using Foundation.Application.Commands.CreateRestAuthorizer;
using Foundation.Application.Commands.CreateRestDeployment;
using Foundation.Application.Commands.CreateRestResource;
using Foundation.Application.Commands.CreateRestStage;
using Foundation.Application.Commands.DeleteRestApi;
using Foundation.Application.Commands.DeleteRestAuthorizer;
using Foundation.Application.Commands.DeleteRestMethod;
using Foundation.Application.Commands.DeleteRestResource;
using Foundation.Application.Commands.DeleteRestStage;
using Foundation.Application.Commands.PutRestMethod;
using Foundation.Application.Commands.UpdateRestApi;
using Foundation.Application.Commands.UpdateRestAuthorizer;
using Foundation.Application.Commands.UpdateRestStage;
using Foundation.Application.Queries.GetRestApi;
using Foundation.Application.Queries.GetRestAuthorizer;
using Foundation.Application.Queries.GetRestMethod;
using Foundation.Application.Queries.GetRestStage;
using Foundation.Application.Queries.ListRestApis;
using Foundation.Application.Queries.ListRestAuthorizers;
using Foundation.Application.Queries.ListRestDeployments;
using Foundation.Application.Queries.ListRestResources;
using Foundation.Application.Queries.ListRestStages;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Api.Controllers;

/// <summary>
/// Provides management of AWS API Gateway REST APIs: listing, reading, creating, updating and deleting.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/services/apigateway")]
public partial class ApiGatewayController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiGatewayController"/> class.
    /// </summary>
    /// <param name="sender">The sender used to dispatch queries.</param>
    /// <param name="logger">The logger.</param>
    public ApiGatewayController(ISender sender, ILogger<ApiGatewayController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Lists the API Gateway REST APIs available on the backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the REST API summaries.</returns>
    [HttpGet("restapis")]
    [ProducesResponseType(typeof(RestApiListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListRestApis(CancellationToken cancellationToken)
    {
        LogHandlingListRestApis();
        var result = await _sender.Send(new ListRestApisQuery(), cancellationToken);
        LogListRestApisHandled(result.IsSuccess);
        return result.Match(
            restApis => Results.Ok(new RestApiListResponse(
                restApis.RestApis
                    .Select(restApi => new RestApiSummaryResponse(
                        restApi.Id,
                        restApi.Name,
                        restApi.Description,
                        restApi.CreatedDate))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Gets the details of an API Gateway REST API.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the REST API details.</returns>
    [HttpGet("restapis/{restApiId}")]
    [ProducesResponseType(typeof(RestApiDetailResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetRestApi(string restApiId, CancellationToken cancellationToken)
    {
        LogHandlingGetRestApi(restApiId);
        var result = await _sender.Send(new GetRestApiQuery(restApiId), cancellationToken);
        LogGetRestApiHandled(result.IsSuccess);
        return result.Match(
            api => Results.Ok(ToDetailResponse(api.RestApi)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a new API Gateway REST API.
    /// </summary>
    /// <param name="request">The REST API configuration to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result carrying the new REST API id.</returns>
    [HttpPost("restapis")]
    [ProducesResponseType(typeof(RestApiCreatedResponse), StatusCodes.Status201Created)]
    public async Task<IResult> CreateRestApi(
        [FromBody] RestApiCreateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreateRestApi(request.Name);
        var result = await _sender.Send(
            new CreateRestApiCommand(
                request.Name,
                request.Description,
                request.Version,
                request.ApiKeySource,
                request.EndpointConfigurationTypes ?? []),
            cancellationToken);
        LogCreateRestApiHandled(result.IsSuccess);
        return result.Match(
            restApiId => Results.Created(
                $"/api/services/apigateway/restapis/{Uri.EscapeDataString(restApiId)}",
                new RestApiCreatedResponse(restApiId)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Updates the configuration of an existing API Gateway REST API.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API to update.</param>
    /// <param name="request">The REST API configuration to apply.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("restapis/{restApiId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> UpdateRestApi(
        string restApiId, [FromBody] RestApiUpdateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingUpdateRestApi(restApiId);
        var result = await _sender.Send(
            new UpdateRestApiCommand(restApiId, request.Name, request.Description),
            cancellationToken);
        LogUpdateRestApiHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes an API Gateway REST API by its identifier. This action cannot be undone.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("restapis/{restApiId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteRestApi(string restApiId, CancellationToken cancellationToken)
    {
        LogHandlingDeleteRestApi(restApiId);
        var result = await _sender.Send(new DeleteRestApiCommand(restApiId), cancellationToken);
        LogDeleteRestApiHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the resource tree of an API Gateway REST API.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API whose resources to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the resource summaries.</returns>
    [HttpGet("restapis/{restApiId}/resources")]
    [ProducesResponseType(typeof(RestResourceListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListRestResources(
        string restApiId, CancellationToken cancellationToken)
    {
        LogHandlingListRestResources(restApiId);
        var result = await _sender.Send(new ListRestResourcesQuery(restApiId), cancellationToken);
        LogListRestResourcesHandled(result.IsSuccess);
        return result.Match(
            resources => Results.Ok(new RestResourceListResponse(
                resources.Resources
                    .Select(ToResourceSummaryResponse)
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a child resource under an existing resource of an API Gateway REST API.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API the resource belongs to.</param>
    /// <param name="request">The resource configuration to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result carrying the new resource id.</returns>
    [HttpPost("restapis/{restApiId}/resources")]
    [ProducesResponseType(typeof(RestResourceCreatedResponse), StatusCodes.Status201Created)]
    public async Task<IResult> CreateRestResource(
        string restApiId,
        [FromBody] RestResourceCreateRequest request,
        CancellationToken cancellationToken)
    {
        LogHandlingCreateRestResource(request.PathPart, restApiId);
        var result = await _sender.Send(
            new CreateRestResourceCommand(restApiId, request.ParentId, request.PathPart),
            cancellationToken);
        LogCreateRestResourceHandled(result.IsSuccess);
        return result.Match(
            resourceId => Results.Created(
                $"/api/services/apigateway/restapis/{Uri.EscapeDataString(restApiId)}/resources/{Uri.EscapeDataString(resourceId)}",
                new RestResourceCreatedResponse(resourceId)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes a resource from an API Gateway REST API. This action cannot be undone.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API the resource belongs to.</param>
    /// <param name="resourceId">The identifier of the resource to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("restapis/{restApiId}/resources/{resourceId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteRestResource(
        string restApiId, string resourceId, CancellationToken cancellationToken)
    {
        LogHandlingDeleteRestResource(resourceId, restApiId);
        var result = await _sender.Send(
            new DeleteRestResourceCommand(restApiId, resourceId), cancellationToken);
        LogDeleteRestResourceHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Gets the configuration of a single HTTP method on a resource.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API.</param>
    /// <param name="resourceId">The identifier of the resource.</param>
    /// <param name="httpMethod">The HTTP verb of the method to read.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the method detail.</returns>
    [HttpGet("restapis/{restApiId}/resources/{resourceId}/methods/{httpMethod}")]
    [ProducesResponseType(typeof(RestMethodDetailResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetRestMethod(
        string restApiId, string resourceId, string httpMethod, CancellationToken cancellationToken)
    {
        LogHandlingGetRestMethod(httpMethod, resourceId, restApiId);
        var result = await _sender.Send(
            new GetRestMethodQuery(restApiId, resourceId, httpMethod), cancellationToken);
        LogGetRestMethodHandled(result.IsSuccess);
        return result.Match(
            method => Results.Ok(ToMethodDetailResponse(method.Method)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates or replaces an HTTP method on a resource.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API.</param>
    /// <param name="resourceId">The identifier of the resource.</param>
    /// <param name="httpMethod">The HTTP verb of the method to configure.</param>
    /// <param name="request">The method configuration to apply.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("restapis/{restApiId}/resources/{resourceId}/methods/{httpMethod}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> PutRestMethod(
        string restApiId,
        string resourceId,
        string httpMethod,
        [FromBody] RestMethodPutRequest request,
        CancellationToken cancellationToken)
    {
        LogHandlingPutRestMethod(httpMethod, resourceId, restApiId);
        var result = await _sender.Send(
            new PutRestMethodCommand(
                restApiId,
                resourceId,
                httpMethod,
                request.AuthorizationType,
                request.AuthorizerId,
                request.ApiKeyRequired,
                request.AuthorizationScopes ?? []),
            cancellationToken);
        LogPutRestMethodHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes an HTTP method from a resource. This action cannot be undone.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API.</param>
    /// <param name="resourceId">The identifier of the resource.</param>
    /// <param name="httpMethod">The HTTP verb of the method to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("restapis/{restApiId}/resources/{resourceId}/methods/{httpMethod}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteRestMethod(
        string restApiId, string resourceId, string httpMethod, CancellationToken cancellationToken)
    {
        LogHandlingDeleteRestMethod(httpMethod, resourceId, restApiId);
        var result = await _sender.Send(
            new DeleteRestMethodCommand(restApiId, resourceId, httpMethod), cancellationToken);
        LogDeleteRestMethodHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the authorizers configured on an API Gateway REST API.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API whose authorizers to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the authorizer summaries.</returns>
    [HttpGet("restapis/{restApiId}/authorizers")]
    [ProducesResponseType(typeof(RestAuthorizerListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListRestAuthorizers(
        string restApiId, CancellationToken cancellationToken)
    {
        LogHandlingListRestAuthorizers(restApiId);
        var result = await _sender.Send(new ListRestAuthorizersQuery(restApiId), cancellationToken);
        LogListRestAuthorizersHandled(result.IsSuccess);
        return result.Match(
            authorizers => Results.Ok(new RestAuthorizerListResponse(
                authorizers.Authorizers
                    .Select(ToAuthorizerSummaryResponse)
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Gets the configuration of a single authorizer on an API Gateway REST API.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API.</param>
    /// <param name="authorizerId">The identifier of the authorizer to read.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the authorizer detail.</returns>
    [HttpGet("restapis/{restApiId}/authorizers/{authorizerId}")]
    [ProducesResponseType(typeof(RestAuthorizerDetailResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetRestAuthorizer(
        string restApiId, string authorizerId, CancellationToken cancellationToken)
    {
        LogHandlingGetRestAuthorizer(authorizerId, restApiId);
        var result = await _sender.Send(
            new GetRestAuthorizerQuery(restApiId, authorizerId), cancellationToken);
        LogGetRestAuthorizerHandled(result.IsSuccess);
        return result.Match(
            authorizer => Results.Ok(ToAuthorizerDetailResponse(authorizer.Authorizer)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a Cognito user pool authorizer on an API Gateway REST API.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API the authorizer belongs to.</param>
    /// <param name="request">The authorizer configuration to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result carrying the new authorizer id.</returns>
    [HttpPost("restapis/{restApiId}/authorizers")]
    [ProducesResponseType(typeof(RestAuthorizerCreatedResponse), StatusCodes.Status201Created)]
    public async Task<IResult> CreateRestAuthorizer(
        string restApiId,
        [FromBody] RestAuthorizerCreateRequest request,
        CancellationToken cancellationToken)
    {
        LogHandlingCreateRestAuthorizer(request.Name, restApiId);
        var result = await _sender.Send(
            new CreateRestAuthorizerCommand(
                restApiId,
                request.Name,
                request.Type,
                request.ProviderARNs ?? [],
                request.IdentitySource),
            cancellationToken);
        LogCreateRestAuthorizerHandled(result.IsSuccess);
        return result.Match(
            authorizerId => Results.Created(
                $"/api/services/apigateway/restapis/{Uri.EscapeDataString(restApiId)}/authorizers/{Uri.EscapeDataString(authorizerId)}",
                new RestAuthorizerCreatedResponse(authorizerId)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Updates an existing Cognito user pool authorizer on an API Gateway REST API.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API the authorizer belongs to.</param>
    /// <param name="authorizerId">The identifier of the authorizer to update.</param>
    /// <param name="request">The authorizer configuration to apply.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("restapis/{restApiId}/authorizers/{authorizerId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> UpdateRestAuthorizer(
        string restApiId,
        string authorizerId,
        [FromBody] RestAuthorizerUpdateRequest request,
        CancellationToken cancellationToken)
    {
        LogHandlingUpdateRestAuthorizer(authorizerId, restApiId);
        var result = await _sender.Send(
            new UpdateRestAuthorizerCommand(
                restApiId,
                authorizerId,
                request.Name,
                request.Type,
                request.ProviderARNs ?? [],
                request.IdentitySource),
            cancellationToken);
        LogUpdateRestAuthorizerHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes an authorizer from an API Gateway REST API. This action cannot be undone.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API the authorizer belongs to.</param>
    /// <param name="authorizerId">The identifier of the authorizer to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("restapis/{restApiId}/authorizers/{authorizerId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteRestAuthorizer(
        string restApiId, string authorizerId, CancellationToken cancellationToken)
    {
        LogHandlingDeleteRestAuthorizer(authorizerId, restApiId);
        var result = await _sender.Send(
            new DeleteRestAuthorizerCommand(restApiId, authorizerId), cancellationToken);
        LogDeleteRestAuthorizerHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the stages configured on an API Gateway REST API.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API whose stages to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the stage summaries.</returns>
    [HttpGet("restapis/{restApiId}/stages")]
    [ProducesResponseType(typeof(RestStageListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListRestStages(
        string restApiId, CancellationToken cancellationToken)
    {
        LogHandlingListRestStages(restApiId);
        var result = await _sender.Send(new ListRestStagesQuery(restApiId), cancellationToken);
        LogListRestStagesHandled(result.IsSuccess);
        return result.Match(
            stages => Results.Ok(new RestStageListResponse(
                stages.Stages
                    .Select(ToStageSummaryResponse)
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Gets the configuration of a single stage on an API Gateway REST API.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API.</param>
    /// <param name="stageName">The name of the stage to read.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the stage detail.</returns>
    [HttpGet("restapis/{restApiId}/stages/{stageName}")]
    [ProducesResponseType(typeof(RestStageDetailResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetRestStage(
        string restApiId, string stageName, CancellationToken cancellationToken)
    {
        LogHandlingGetRestStage(stageName, restApiId);
        var result = await _sender.Send(
            new GetRestStageQuery(restApiId, stageName), cancellationToken);
        LogGetRestStageHandled(result.IsSuccess);
        return result.Match(
            stage => Results.Ok(ToStageDetailResponse(stage.Stage)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the deployments of an API Gateway REST API.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API whose deployments to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the deployment summaries.</returns>
    [HttpGet("restapis/{restApiId}/deployments")]
    [ProducesResponseType(typeof(RestDeploymentListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListRestDeployments(
        string restApiId, CancellationToken cancellationToken)
    {
        LogHandlingListRestDeployments(restApiId);
        var result = await _sender.Send(new ListRestDeploymentsQuery(restApiId), cancellationToken);
        LogListRestDeploymentsHandled(result.IsSuccess);
        return result.Match(
            deployments => Results.Ok(new RestDeploymentListResponse(
                deployments.Deployments
                    .Select(ToDeploymentSummaryResponse)
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a deployment of an API Gateway REST API, optionally creating a stage that points at it.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API the deployment belongs to.</param>
    /// <param name="request">The deployment configuration to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result carrying the new deployment id.</returns>
    [HttpPost("restapis/{restApiId}/deployments")]
    [ProducesResponseType(typeof(RestDeploymentCreatedResponse), StatusCodes.Status201Created)]
    public async Task<IResult> CreateRestDeployment(
        string restApiId,
        [FromBody] RestDeploymentCreateRequest request,
        CancellationToken cancellationToken)
    {
        LogHandlingCreateRestDeployment(restApiId);
        var result = await _sender.Send(
            new CreateRestDeploymentCommand(
                restApiId,
                request.StageName,
                request.Description),
            cancellationToken);
        LogCreateRestDeploymentHandled(result.IsSuccess);
        return result.Match(
            deploymentId => Results.Created(
                $"/api/services/apigateway/restapis/{Uri.EscapeDataString(restApiId)}/deployments/{Uri.EscapeDataString(deploymentId)}",
                new RestDeploymentCreatedResponse(deploymentId)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a stage on an API Gateway REST API that points at an existing deployment.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API the stage belongs to.</param>
    /// <param name="request">The stage configuration to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result carrying the new stage name.</returns>
    [HttpPost("restapis/{restApiId}/stages")]
    [ProducesResponseType(typeof(RestStageCreatedResponse), StatusCodes.Status201Created)]
    public async Task<IResult> CreateRestStage(
        string restApiId,
        [FromBody] RestStageCreateRequest request,
        CancellationToken cancellationToken)
    {
        LogHandlingCreateRestStage(request.StageName, restApiId);
        var result = await _sender.Send(
            new CreateRestStageCommand(
                restApiId,
                request.StageName,
                request.DeploymentId,
                request.Description,
                request.Variables ?? new Dictionary<string, string>()),
            cancellationToken);
        LogCreateRestStageHandled(result.IsSuccess);
        return result.Match(
            stageName => Results.Created(
                $"/api/services/apigateway/restapis/{Uri.EscapeDataString(restApiId)}/stages/{Uri.EscapeDataString(stageName)}",
                new RestStageCreatedResponse(stageName)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Updates an existing stage on an API Gateway REST API.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API the stage belongs to.</param>
    /// <param name="stageName">The name of the stage to update.</param>
    /// <param name="request">The stage configuration to apply.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("restapis/{restApiId}/stages/{stageName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> UpdateRestStage(
        string restApiId,
        string stageName,
        [FromBody] RestStageUpdateRequest request,
        CancellationToken cancellationToken)
    {
        LogHandlingUpdateRestStage(stageName, restApiId);
        var result = await _sender.Send(
            new UpdateRestStageCommand(
                restApiId,
                stageName,
                request.Description,
                request.Variables ?? new Dictionary<string, string>()),
            cancellationToken);
        LogUpdateRestStageHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes a stage from an API Gateway REST API. This action cannot be undone.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API the stage belongs to.</param>
    /// <param name="stageName">The name of the stage to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("restapis/{restApiId}/stages/{stageName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteRestStage(
        string restApiId, string stageName, CancellationToken cancellationToken)
    {
        LogHandlingDeleteRestStage(stageName, restApiId);
        var result = await _sender.Send(
            new DeleteRestStageCommand(restApiId, stageName), cancellationToken);
        LogDeleteRestStageHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    private static RestApiDetailResponse ToDetailResponse(Foundation.Domain.ApiGateway.RestApiDetail api)
        => new(
            api.Id,
            api.Name,
            api.Description,
            api.Version,
            api.ApiKeySource,
            api.EndpointConfigurationTypes,
            api.BinaryMediaTypes,
            api.CreatedDate);

    private static RestResourceSummaryResponse ToResourceSummaryResponse(
        Foundation.Domain.ApiGateway.RestResourceSummary resource)
        => new(
            resource.Id,
            resource.ParentId,
            resource.PathPart,
            resource.Path,
            resource.ResourceMethods);

    private static RestMethodDetailResponse ToMethodDetailResponse(
        Foundation.Domain.ApiGateway.RestMethodDetail method)
        => new(
            method.ResourceId,
            method.HttpMethod,
            method.AuthorizationType,
            method.AuthorizerId,
            method.ApiKeyRequired,
            method.AuthorizationScopes);

    private static RestAuthorizerSummaryResponse ToAuthorizerSummaryResponse(
        Foundation.Domain.ApiGateway.RestAuthorizerSummary authorizer)
        => new(
            authorizer.Id,
            authorizer.Name,
            authorizer.Type);

    private static RestAuthorizerDetailResponse ToAuthorizerDetailResponse(
        Foundation.Domain.ApiGateway.RestAuthorizerDetail authorizer)
        => new(
            authorizer.Id,
            authorizer.Name,
            authorizer.Type,
            authorizer.ProviderARNs,
            authorizer.IdentitySource,
            authorizer.AuthType);

    private static RestStageSummaryResponse ToStageSummaryResponse(
        Foundation.Domain.ApiGateway.RestStageSummary stage)
        => new(
            stage.StageName,
            stage.DeploymentId,
            stage.CreatedDate);

    private static RestStageDetailResponse ToStageDetailResponse(
        Foundation.Domain.ApiGateway.RestStageDetail stage)
        => new(
            stage.StageName,
            stage.DeploymentId,
            stage.Description,
            stage.CacheClusterEnabled,
            stage.Variables,
            stage.CreatedDate,
            stage.LastUpdatedDate);

    private static RestDeploymentSummaryResponse ToDeploymentSummaryResponse(
        Foundation.Domain.ApiGateway.RestDeploymentSummary deployment)
        => new(
            deployment.Id,
            deployment.Description,
            deployment.CreatedDate);

    [LoggerMessage(LogLevel.Trace, "Listing API Gateway REST APIs.")]
    private partial void LogHandlingListRestApis();

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API list handled. Success: {Success}")]
    private partial void LogListRestApisHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Getting API Gateway REST API {RestApiId}.")]
    private partial void LogHandlingGetRestApi(string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API get handled. Success: {Success}")]
    private partial void LogGetRestApiHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Creating API Gateway REST API {Name}.")]
    private partial void LogHandlingCreateRestApi(string name);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API create handled. Success: {Success}")]
    private partial void LogCreateRestApiHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Updating API Gateway REST API {RestApiId}.")]
    private partial void LogHandlingUpdateRestApi(string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API update handled. Success: {Success}")]
    private partial void LogUpdateRestApiHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Deleting API Gateway REST API {RestApiId}.")]
    private partial void LogHandlingDeleteRestApi(string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API delete handled. Success: {Success}")]
    private partial void LogDeleteRestApiHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Listing API Gateway REST API resources for {RestApiId}.")]
    private partial void LogHandlingListRestResources(string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API resource list handled. Success: {Success}")]
    private partial void LogListRestResourcesHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Creating API Gateway REST API resource {PathPart} on {RestApiId}.")]
    private partial void LogHandlingCreateRestResource(string pathPart, string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API resource create handled. Success: {Success}")]
    private partial void LogCreateRestResourceHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Deleting API Gateway REST API resource {ResourceId} on {RestApiId}.")]
    private partial void LogHandlingDeleteRestResource(string resourceId, string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API resource delete handled. Success: {Success}")]
    private partial void LogDeleteRestResourceHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Getting API Gateway REST API method {HttpMethod} on {ResourceId} of {RestApiId}.")]
    private partial void LogHandlingGetRestMethod(string httpMethod, string resourceId, string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API method get handled. Success: {Success}")]
    private partial void LogGetRestMethodHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Configuring API Gateway REST API method {HttpMethod} on {ResourceId} of {RestApiId}.")]
    private partial void LogHandlingPutRestMethod(string httpMethod, string resourceId, string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API method put handled. Success: {Success}")]
    private partial void LogPutRestMethodHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Deleting API Gateway REST API method {HttpMethod} on {ResourceId} of {RestApiId}.")]
    private partial void LogHandlingDeleteRestMethod(string httpMethod, string resourceId, string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API method delete handled. Success: {Success}")]
    private partial void LogDeleteRestMethodHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Listing API Gateway REST API authorizers for {RestApiId}.")]
    private partial void LogHandlingListRestAuthorizers(string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API authorizer list handled. Success: {Success}")]
    private partial void LogListRestAuthorizersHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Getting API Gateway REST API authorizer {AuthorizerId} of {RestApiId}.")]
    private partial void LogHandlingGetRestAuthorizer(string authorizerId, string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API authorizer get handled. Success: {Success}")]
    private partial void LogGetRestAuthorizerHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Creating API Gateway REST API authorizer {Name} on {RestApiId}.")]
    private partial void LogHandlingCreateRestAuthorizer(string name, string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API authorizer create handled. Success: {Success}")]
    private partial void LogCreateRestAuthorizerHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Updating API Gateway REST API authorizer {AuthorizerId} on {RestApiId}.")]
    private partial void LogHandlingUpdateRestAuthorizer(string authorizerId, string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API authorizer update handled. Success: {Success}")]
    private partial void LogUpdateRestAuthorizerHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Deleting API Gateway REST API authorizer {AuthorizerId} on {RestApiId}.")]
    private partial void LogHandlingDeleteRestAuthorizer(string authorizerId, string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API authorizer delete handled. Success: {Success}")]
    private partial void LogDeleteRestAuthorizerHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Listing API Gateway REST API stages for {RestApiId}.")]
    private partial void LogHandlingListRestStages(string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API stage list handled. Success: {Success}")]
    private partial void LogListRestStagesHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Getting API Gateway REST API stage {StageName} of {RestApiId}.")]
    private partial void LogHandlingGetRestStage(string stageName, string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API stage get handled. Success: {Success}")]
    private partial void LogGetRestStageHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Listing API Gateway REST API deployments for {RestApiId}.")]
    private partial void LogHandlingListRestDeployments(string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API deployment list handled. Success: {Success}")]
    private partial void LogListRestDeploymentsHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Creating API Gateway REST API deployment on {RestApiId}.")]
    private partial void LogHandlingCreateRestDeployment(string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API deployment create handled. Success: {Success}")]
    private partial void LogCreateRestDeploymentHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Creating API Gateway REST API stage {StageName} on {RestApiId}.")]
    private partial void LogHandlingCreateRestStage(string stageName, string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API stage create handled. Success: {Success}")]
    private partial void LogCreateRestStageHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Updating API Gateway REST API stage {StageName} on {RestApiId}.")]
    private partial void LogHandlingUpdateRestStage(string stageName, string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API stage update handled. Success: {Success}")]
    private partial void LogUpdateRestStageHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Deleting API Gateway REST API stage {StageName} on {RestApiId}.")]
    private partial void LogHandlingDeleteRestStage(string stageName, string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API stage delete handled. Success: {Success}")]
    private partial void LogDeleteRestStageHandled(bool success);
}
