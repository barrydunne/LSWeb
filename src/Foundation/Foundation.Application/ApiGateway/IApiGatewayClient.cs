using AspNet.KickStarter.FunctionalResult;
using Foundation.Domain.ApiGateway;

namespace Foundation.Application.ApiGateway;

/// <summary>
/// Abstracts the API Gateway operations the application needs so the handlers stay free of any
/// direct AWS SDK dependency. The implementation flows every call through the resilient AWS gateway
/// and translates failures into a <see cref="Result"/> rather than throwing.
/// </summary>
public interface IApiGatewayClient
{
    /// <summary>
    /// List the REST APIs available on the backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The REST APIs, or an error when the backend cannot be reached.</returns>
    Task<Result<IReadOnlyList<RestApi>>> ListRestApisAsync(
        CancellationToken cancellationToken);

    /// <summary>
    /// Read the full configuration of a single REST API.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The REST API detail, or an error when the REST API cannot be read.</returns>
    Task<Result<RestApiDetail>> GetRestApiAsync(
        string restApiId, CancellationToken cancellationToken);

    /// <summary>
    /// Create a new REST API from the supplied specification.
    /// </summary>
    /// <param name="specification">The desired configuration of the REST API to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The identifier of the created REST API, or an error when it cannot be created.</returns>
    Task<Result<string>> CreateRestApiAsync(
        RestApiSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Update the configuration of an existing REST API.
    /// </summary>
    /// <param name="specification">The desired configuration of the REST API to update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error when the REST API cannot be updated.</returns>
    Task<Result> UpdateRestApiAsync(
        RestApiSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Delete a REST API by its identifier. This action cannot be undone.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error when the REST API cannot be deleted.</returns>
    Task<Result> DeleteRestApiAsync(
        string restApiId, CancellationToken cancellationToken);

    /// <summary>
    /// List the resource tree of a REST API.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The resources of the REST API, or an error when they cannot be read.</returns>
    Task<Result<IReadOnlyList<RestResourceSummary>>> ListResourcesAsync(
        string restApiId, CancellationToken cancellationToken);

    /// <summary>
    /// Create a child resource under an existing resource of a REST API.
    /// </summary>
    /// <param name="specification">The desired configuration of the resource to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The identifier of the created resource, or an error when it cannot be created.</returns>
    Task<Result<string>> CreateResourceAsync(
        RestResourceSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Delete a resource from a REST API. This action cannot be undone.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API the resource belongs to.</param>
    /// <param name="resourceId">The identifier of the resource to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error when the resource cannot be deleted.</returns>
    Task<Result> DeleteResourceAsync(
        string restApiId, string resourceId, CancellationToken cancellationToken);

    /// <summary>
    /// Read the configuration of a single HTTP method on a resource.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API.</param>
    /// <param name="resourceId">The identifier of the resource.</param>
    /// <param name="httpMethod">The HTTP verb of the method to read.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The method detail, or an error when the method cannot be read.</returns>
    Task<Result<RestMethodDetail>> GetMethodAsync(
        string restApiId, string resourceId, string httpMethod, CancellationToken cancellationToken);

    /// <summary>
    /// Create or replace an HTTP method on a resource, wiring a minimal MOCK integration so the
    /// method is callable.
    /// </summary>
    /// <param name="specification">The desired configuration of the method.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error when the method cannot be configured.</returns>
    Task<Result> PutMethodAsync(
        RestMethodSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Delete an HTTP method from a resource. This action cannot be undone.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API.</param>
    /// <param name="resourceId">The identifier of the resource.</param>
    /// <param name="httpMethod">The HTTP verb of the method to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error when the method cannot be deleted.</returns>
    Task<Result> DeleteMethodAsync(
        string restApiId, string resourceId, string httpMethod, CancellationToken cancellationToken);

    /// <summary>
    /// Test invoke an HTTP method on a REST API resource.
    /// </summary>
    /// <param name="specification">The invocation request payload.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The invocation result, or an error when the invocation cannot be executed.</returns>
    Task<Result<RestMethodTestInvocationResult>> TestInvokeMethodAsync(
        RestMethodTestInvocationSpecification specification,
        CancellationToken cancellationToken);

    /// <summary>
    /// Read the CORS policy configured on a resource, reported as not enabled when no preflight
    /// (OPTIONS) method is present.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API.</param>
    /// <param name="resourceId">The identifier of the resource whose CORS policy to read.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The CORS configuration, or an error when it cannot be read.</returns>
    Task<Result<RestCorsConfiguration>> GetCorsAsync(
        string restApiId, string resourceId, CancellationToken cancellationToken);

    /// <summary>
    /// Configure the CORS policy on a resource by wiring an OPTIONS preflight method backed by a
    /// MOCK integration that returns the configured Access-Control headers.
    /// </summary>
    /// <param name="specification">The desired CORS policy.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error when the CORS policy cannot be configured.</returns>
    Task<Result> ConfigureCorsAsync(
        RestCorsSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// List the authorizers configured on a REST API.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The authorizers of the REST API, or an error when they cannot be read.</returns>
    Task<Result<IReadOnlyList<RestAuthorizerSummary>>> ListAuthorizersAsync(
        string restApiId, CancellationToken cancellationToken);

    /// <summary>
    /// Read the full configuration of a single authorizer.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API.</param>
    /// <param name="authorizerId">The identifier of the authorizer.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The authorizer detail, or an error when it cannot be read.</returns>
    Task<Result<RestAuthorizerDetail>> GetAuthorizerAsync(
        string restApiId, string authorizerId, CancellationToken cancellationToken);

    /// <summary>
    /// Create a new Cognito user pool authorizer from the supplied specification.
    /// </summary>
    /// <param name="specification">The desired configuration of the authorizer to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The identifier of the created authorizer, or an error when it cannot be created.</returns>
    Task<Result<string>> CreateAuthorizerAsync(
        RestAuthorizerSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Create a new OAuth/JWT token authorizer from the supplied specification.
    /// </summary>
    /// <param name="specification">The desired configuration of the token authorizer to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The identifier of the created authorizer, or an error when it cannot be created.</returns>
    Task<Result<string>> CreateTokenAuthorizerAsync(
        RestTokenAuthorizerSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Update the configuration of an existing authorizer.
    /// </summary>
    /// <param name="specification">The desired configuration of the authorizer to update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error when the authorizer cannot be updated.</returns>
    Task<Result> UpdateAuthorizerAsync(
        RestAuthorizerSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Delete an authorizer from a REST API. This action cannot be undone.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API the authorizer belongs to.</param>
    /// <param name="authorizerId">The identifier of the authorizer to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error when the authorizer cannot be deleted.</returns>
    Task<Result> DeleteAuthorizerAsync(
        string restApiId, string authorizerId, CancellationToken cancellationToken);

    /// <summary>
    /// List the stages configured on a REST API.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The stages of the REST API, or an error when they cannot be read.</returns>
    Task<Result<IReadOnlyList<RestStageSummary>>> ListStagesAsync(
        string restApiId, CancellationToken cancellationToken);

    /// <summary>
    /// Read the full configuration of a single stage.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API.</param>
    /// <param name="stageName">The name of the stage.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The stage detail, or an error when it cannot be read.</returns>
    Task<Result<RestStageDetail>> GetStageAsync(
        string restApiId, string stageName, CancellationToken cancellationToken);

    /// <summary>
    /// List the deployments of a REST API.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The deployments of the REST API, or an error when they cannot be read.</returns>
    Task<Result<IReadOnlyList<RestDeploymentSummary>>> ListDeploymentsAsync(
        string restApiId, CancellationToken cancellationToken);

    /// <summary>
    /// Create a new deployment of a REST API, optionally creating a stage that points at it.
    /// </summary>
    /// <param name="specification">The desired configuration of the deployment to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The identifier of the created deployment, or an error when it cannot be created.</returns>
    Task<Result<string>> CreateDeploymentAsync(
        RestDeploymentSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Create a new stage on a REST API that points at an existing deployment.
    /// </summary>
    /// <param name="specification">The desired configuration of the stage to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The name of the created stage, or an error when it cannot be created.</returns>
    Task<Result<string>> CreateStageAsync(
        RestStageSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Update the configuration of an existing stage.
    /// </summary>
    /// <param name="specification">The desired configuration of the stage to update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error when the stage cannot be updated.</returns>
    Task<Result> UpdateStageAsync(
        RestStageSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Delete a stage from a REST API. This action cannot be undone.
    /// </summary>
    /// <param name="restApiId">The identifier of the REST API the stage belongs to.</param>
    /// <param name="stageName">The name of the stage to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error when the stage cannot be deleted.</returns>
    Task<Result> DeleteStageAsync(
        string restApiId, string stageName, CancellationToken cancellationToken);
}
