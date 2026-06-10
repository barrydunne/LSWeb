namespace Foundation.Api.Models;

/// <summary>
/// The API Gateway REST APIs available on the backend.
/// </summary>
/// <param name="RestApis">The REST API summaries, ordered as returned by the backend.</param>
public sealed record RestApiListResponse(
    IReadOnlyList<RestApiSummaryResponse> RestApis);

/// <summary>
/// A concise view of an API Gateway REST API as it appears in a list.
/// </summary>
/// <param name="Id">The identifier that uniquely identifies the REST API.</param>
/// <param name="Name">The name of the REST API.</param>
/// <param name="Description">An optional human-readable description, or <c>null</c> when none is set.</param>
/// <param name="CreatedDate">The UTC creation timestamp, or <c>null</c> when not reported.</param>
public sealed record RestApiSummaryResponse(
    string Id,
    string Name,
    string? Description,
    DateTimeOffset? CreatedDate);

/// <summary>
/// A detailed view of an API Gateway REST API.
/// </summary>
/// <param name="Id">The identifier that uniquely identifies the REST API.</param>
/// <param name="Name">The name of the REST API.</param>
/// <param name="Description">An optional human-readable description, or <c>null</c> when none is set.</param>
/// <param name="Version">An optional version identifier, or <c>null</c> when none is set.</param>
/// <param name="ApiKeySource">The source of the API key for metering requests, or <c>null</c> when not set.</param>
/// <param name="EndpointConfigurationTypes">The endpoint types configured for the REST API.</param>
/// <param name="BinaryMediaTypes">The binary media types supported by the REST API.</param>
/// <param name="CreatedDate">The UTC creation timestamp, or <c>null</c> when not reported.</param>
public sealed record RestApiDetailResponse(
    string Id,
    string Name,
    string? Description,
    string? Version,
    string? ApiKeySource,
    IReadOnlyList<string> EndpointConfigurationTypes,
    IReadOnlyList<string> BinaryMediaTypes,
    DateTimeOffset? CreatedDate);

/// <summary>
/// The payload used to create an API Gateway REST API.
/// </summary>
/// <param name="Name">The name of the REST API.</param>
/// <param name="Description">An optional human-readable description.</param>
/// <param name="Version">An optional version identifier.</param>
/// <param name="ApiKeySource">An optional source of the API key for metering requests.</param>
/// <param name="EndpointConfigurationTypes">The endpoint types to configure for the REST API.</param>
public sealed record RestApiCreateRequest(
    string Name,
    string? Description,
    string? Version,
    string? ApiKeySource,
    IReadOnlyList<string>? EndpointConfigurationTypes);

/// <summary>
/// The payload used to update an API Gateway REST API.
/// </summary>
/// <param name="Name">The name of the REST API.</param>
/// <param name="Description">An optional human-readable description.</param>
public sealed record RestApiUpdateRequest(
    string Name,
    string? Description);

/// <summary>
/// The result of creating an API Gateway REST API.
/// </summary>
/// <param name="Id">The identifier of the newly created REST API.</param>
public sealed record RestApiCreatedResponse(string Id);

/// <summary>
/// The resources that make up the tree of an API Gateway REST API.
/// </summary>
/// <param name="Resources">The resource summaries, ordered as returned by the backend.</param>
public sealed record RestResourceListResponse(
    IReadOnlyList<RestResourceSummaryResponse> Resources);

/// <summary>
/// A node in the resource tree of an API Gateway REST API.
/// </summary>
/// <param name="Id">The identifier that uniquely identifies the resource.</param>
/// <param name="ParentId">The identifier of the parent resource, or <c>null</c> for the root resource.</param>
/// <param name="PathPart">The last path segment of the resource, or <c>null</c> for the root resource.</param>
/// <param name="Path">The full path of the resource from the root, for example <c>/items/{id}</c>.</param>
/// <param name="ResourceMethods">The HTTP methods configured on the resource.</param>
public sealed record RestResourceSummaryResponse(
    string Id,
    string? ParentId,
    string? PathPart,
    string Path,
    IReadOnlyList<string> ResourceMethods);

/// <summary>
/// The payload used to create an API Gateway REST API resource.
/// </summary>
/// <param name="ParentId">The identifier of the parent resource the new resource hangs from.</param>
/// <param name="PathPart">The last path segment of the resource to create, for example <c>items</c>.</param>
public sealed record RestResourceCreateRequest(
    string ParentId,
    string PathPart);

/// <summary>
/// The result of creating an API Gateway REST API resource.
/// </summary>
/// <param name="Id">The identifier of the newly created resource.</param>
public sealed record RestResourceCreatedResponse(string Id);

/// <summary>
/// The configuration of a single HTTP method on an API Gateway REST API resource.
/// </summary>
/// <param name="ResourceId">The identifier of the resource the method belongs to.</param>
/// <param name="HttpMethod">The HTTP verb of the method (for example <c>GET</c>, <c>POST</c> or <c>ANY</c>).</param>
/// <param name="AuthorizationType">The authorization type of the method (for example <c>NONE</c> or <c>COGNITO_USER_POOLS</c>).</param>
/// <param name="AuthorizerId">The identifier of the authorizer applied to the method, or <c>null</c> when none is set.</param>
/// <param name="ApiKeyRequired">Whether an API key is required to call the method.</param>
/// <param name="AuthorizationScopes">The authorization scopes required by the method, when applicable.</param>
/// <param name="IntegrationType">The integration type configured for backend forwarding.</param>
/// <param name="IntegrationUri">The integration URI/ARN configured for backend forwarding, or <c>null</c> when none is set.</param>
public sealed record RestMethodDetailResponse(
    string ResourceId,
    string HttpMethod,
    string AuthorizationType,
    string? AuthorizerId,
    bool ApiKeyRequired,
    IReadOnlyList<string> AuthorizationScopes,
    string IntegrationType,
    string? IntegrationUri);

/// <summary>
/// The payload used to create or replace an HTTP method on an API Gateway REST API resource.
/// </summary>
/// <param name="AuthorizationType">The authorization type of the method (for example <c>NONE</c> or <c>COGNITO_USER_POOLS</c>).</param>
/// <param name="AuthorizerId">The identifier of the authorizer to apply, or <c>null</c> when none is required.</param>
/// <param name="ApiKeyRequired">Whether an API key is required to call the method.</param>
/// <param name="AuthorizationScopes">The authorization scopes required by the method, when applicable.</param>
/// <param name="IntegrationType">The integration type for backend forwarding (for example <c>MOCK</c>, <c>HTTP</c> or <c>AWS_PROXY</c>).</param>
/// <param name="IntegrationUri">The integration URI/ARN to target, or <c>null</c> when the selected integration type does not require one.</param>
public sealed record RestMethodPutRequest(
    string AuthorizationType,
    string? AuthorizerId,
    bool ApiKeyRequired,
    IReadOnlyList<string>? AuthorizationScopes,
    string IntegrationType,
    string? IntegrationUri);

/// <summary>
/// The payload used to test invoke an HTTP method on an API Gateway REST API resource.
/// </summary>
/// <param name="PathWithQueryString">The request path and optional query string, for example <c>/orders?id=1</c>.</param>
/// <param name="Headers">The request headers to include.</param>
/// <param name="QueryStringParameters">Additional query string parameters to include.</param>
/// <param name="Body">The optional request body.</param>
/// <param name="StageVariables">The optional stage variables used during invocation.</param>
public sealed record RestMethodTestInvokeRequest(
    string PathWithQueryString,
    IReadOnlyDictionary<string, string>? Headers,
    IReadOnlyDictionary<string, string>? QueryStringParameters,
    string? Body,
    IReadOnlyDictionary<string, string>? StageVariables);

/// <summary>
/// The result of testing an HTTP method invocation on an API Gateway REST API resource.
/// </summary>
/// <param name="StatusCode">The returned HTTP status code.</param>
/// <param name="LatencyMilliseconds">The invocation latency in milliseconds.</param>
/// <param name="Headers">The returned response headers.</param>
/// <param name="Body">The returned response body.</param>
/// <param name="Log">The execution log output, or <c>null</c> when unavailable.</param>
public sealed record RestMethodTestInvokeResponse(
    int StatusCode,
    int LatencyMilliseconds,
    IReadOnlyDictionary<string, string> Headers,
    string Body,
    string? Log);

/// <summary>
/// The collection of authorizers configured on an API Gateway REST API.
/// </summary>
/// <param name="Authorizers">The authorizer summaries.</param>
public sealed record RestAuthorizerListResponse(
    IReadOnlyList<RestAuthorizerSummaryResponse> Authorizers);

/// <summary>
/// A summary of an API Gateway REST API authorizer.
/// </summary>
/// <param name="Id">The identifier of the authorizer.</param>
/// <param name="Name">The name of the authorizer.</param>
/// <param name="Type">The authorizer type (for example <c>COGNITO_USER_POOLS</c>).</param>
public sealed record RestAuthorizerSummaryResponse(
    string Id,
    string Name,
    string Type);

/// <summary>
/// The full configuration of an API Gateway REST API authorizer.
/// </summary>
/// <param name="Id">The identifier of the authorizer.</param>
/// <param name="Name">The name of the authorizer.</param>
/// <param name="Type">The authorizer type (for example <c>COGNITO_USER_POOLS</c>).</param>
/// <param name="ProviderARNs">The Cognito user pool ARNs the authorizer trusts.</param>
/// <param name="IdentitySource">The request location the identity token is read from, or <c>null</c> when not set.</param>
/// <param name="AuthType">The optional authorization type label reported by the backend, or <c>null</c> when not set.</param>
public sealed record RestAuthorizerDetailResponse(
    string Id,
    string Name,
    string Type,
    IReadOnlyList<string> ProviderARNs,
    string? IdentitySource,
    string? AuthType);

/// <summary>
/// The payload used to create a Cognito user pool authorizer on an API Gateway REST API.
/// </summary>
/// <param name="Name">The name of the authorizer.</param>
/// <param name="Type">The authorizer type. Only <c>COGNITO_USER_POOLS</c> is supported.</param>
/// <param name="ProviderARNs">The Cognito user pool ARNs the authorizer trusts.</param>
/// <param name="IdentitySource">The request location the identity token is read from, or <c>null</c> to use the default.</param>
public sealed record RestAuthorizerCreateRequest(
    string Name,
    string Type,
    IReadOnlyList<string>? ProviderARNs,
    string? IdentitySource);

/// <summary>
/// The payload used to create an OAuth/JWT token authorizer on an API Gateway REST API.
/// </summary>
/// <param name="Name">The name of the authorizer.</param>
/// <param name="Issuer">The OIDC issuer the tokens are expected to originate from.</param>
/// <param name="Audience">The audience the tokens are expected to be issued for.</param>
/// <param name="IdentitySource">The request location the bearer token is read from.</param>
/// <param name="AuthorizerUri">The invocation URI of the function that validates the bearer token.</param>
public sealed record RestTokenAuthorizerCreateRequest(
    string Name,
    string Issuer,
    string Audience,
    string IdentitySource,
    string AuthorizerUri);

/// <summary>
/// The payload used to update a Cognito user pool authorizer on an API Gateway REST API.
/// </summary>
/// <param name="Name">The name of the authorizer.</param>
/// <param name="Type">The authorizer type. Only <c>COGNITO_USER_POOLS</c> is supported.</param>
/// <param name="ProviderARNs">The Cognito user pool ARNs the authorizer trusts.</param>
/// <param name="IdentitySource">The request location the identity token is read from, or <c>null</c> to use the default.</param>
public sealed record RestAuthorizerUpdateRequest(
    string Name,
    string Type,
    IReadOnlyList<string>? ProviderARNs,
    string? IdentitySource);

/// <summary>
/// The result of creating an API Gateway REST API authorizer.
/// </summary>
/// <param name="Id">The identifier of the newly created authorizer.</param>
public sealed record RestAuthorizerCreatedResponse(string Id);

/// <summary>
/// The stages configured on an API Gateway REST API.
/// </summary>
/// <param name="Stages">The stage summaries, ordered as returned by the backend.</param>
public sealed record RestStageListResponse(
    IReadOnlyList<RestStageSummaryResponse> Stages);

/// <summary>
/// A concise view of an API Gateway REST API stage as it appears in a list.
/// </summary>
/// <param name="StageName">The name of the stage.</param>
/// <param name="DeploymentId">The identifier of the deployment the stage points at.</param>
/// <param name="CreatedDate">The UTC creation timestamp, or <c>null</c> when not reported.</param>
public sealed record RestStageSummaryResponse(
    string StageName,
    string DeploymentId,
    DateTimeOffset? CreatedDate);

/// <summary>
/// A detailed view of an API Gateway REST API stage.
/// </summary>
/// <param name="StageName">The name of the stage.</param>
/// <param name="DeploymentId">The identifier of the deployment the stage points at.</param>
/// <param name="Description">An optional human-readable description, or <c>null</c> when none is set.</param>
/// <param name="CacheClusterEnabled">Whether a cache cluster is enabled for the stage.</param>
/// <param name="Variables">The stage variables, keyed by name.</param>
/// <param name="CreatedDate">The UTC creation timestamp, or <c>null</c> when not reported.</param>
/// <param name="LastUpdatedDate">The UTC last-updated timestamp, or <c>null</c> when not reported.</param>
public sealed record RestStageDetailResponse(
    string StageName,
    string DeploymentId,
    string? Description,
    bool CacheClusterEnabled,
    IReadOnlyDictionary<string, string> Variables,
    DateTimeOffset? CreatedDate,
    DateTimeOffset? LastUpdatedDate);

/// <summary>
/// The payload used to create an API Gateway REST API stage.
/// </summary>
/// <param name="StageName">The name of the stage.</param>
/// <param name="DeploymentId">The identifier of the deployment the stage points at.</param>
/// <param name="Description">An optional human-readable description.</param>
/// <param name="Variables">The stage variables to set, keyed by name.</param>
public sealed record RestStageCreateRequest(
    string StageName,
    string DeploymentId,
    string? Description,
    IReadOnlyDictionary<string, string>? Variables);

/// <summary>
/// The payload used to update an API Gateway REST API stage.
/// </summary>
/// <param name="Description">An optional human-readable description.</param>
/// <param name="Variables">The stage variables to set, keyed by name.</param>
public sealed record RestStageUpdateRequest(
    string? Description,
    IReadOnlyDictionary<string, string>? Variables);

/// <summary>
/// The result of creating an API Gateway REST API stage.
/// </summary>
/// <param name="StageName">The name of the newly created stage.</param>
public sealed record RestStageCreatedResponse(string StageName);

/// <summary>
/// The deployments of an API Gateway REST API.
/// </summary>
/// <param name="Deployments">The deployment summaries, ordered as returned by the backend.</param>
public sealed record RestDeploymentListResponse(
    IReadOnlyList<RestDeploymentSummaryResponse> Deployments);

/// <summary>
/// A concise view of an API Gateway REST API deployment as it appears in a list.
/// </summary>
/// <param name="Id">The identifier that uniquely identifies the deployment.</param>
/// <param name="Description">An optional human-readable description, or <c>null</c> when none is set.</param>
/// <param name="CreatedDate">The UTC creation timestamp, or <c>null</c> when not reported.</param>
public sealed record RestDeploymentSummaryResponse(
    string Id,
    string? Description,
    DateTimeOffset? CreatedDate);

/// <summary>
/// The payload used to create an API Gateway REST API deployment.
/// </summary>
/// <param name="StageName">An optional stage name to create and point at the new deployment.</param>
/// <param name="Description">An optional human-readable description.</param>
public sealed record RestDeploymentCreateRequest(
    string? StageName,
    string? Description);

/// <summary>
/// The result of creating an API Gateway REST API deployment.
/// </summary>
/// <param name="Id">The identifier of the newly created deployment.</param>
public sealed record RestDeploymentCreatedResponse(string Id);

/// <summary>
/// The CORS policy configured on an API Gateway REST API resource.
/// </summary>
/// <param name="ResourceId">The identifier of the resource the policy applies to.</param>
/// <param name="Enabled">Whether a CORS preflight (OPTIONS) policy is configured on the resource.</param>
/// <param name="AllowOrigins">The origins the policy allows.</param>
/// <param name="AllowMethods">The HTTP methods the policy allows.</param>
/// <param name="AllowHeaders">The request headers the policy allows.</param>
public sealed record RestCorsResponse(
    string ResourceId,
    bool Enabled,
    IReadOnlyList<string> AllowOrigins,
    IReadOnlyList<string> AllowMethods,
    IReadOnlyList<string> AllowHeaders);

/// <summary>
/// The payload used to configure the CORS policy on an API Gateway REST API resource.
/// </summary>
/// <param name="AllowOrigins">The origins to allow.</param>
/// <param name="AllowMethods">The HTTP methods to allow.</param>
/// <param name="AllowHeaders">The request headers to allow.</param>
public sealed record RestCorsConfigureRequest(
    IReadOnlyList<string>? AllowOrigins,
    IReadOnlyList<string>? AllowMethods,
    IReadOnlyList<string>? AllowHeaders);
