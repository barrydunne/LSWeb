namespace Foundation.Api.Models;

/// <summary>
/// A list of Amazon API Gateway v2 API summaries.
/// </summary>
/// <param name="Apis">The API summaries.</param>
public sealed record HttpApiListResponse(IReadOnlyList<HttpApiSummaryResponse> Apis);

/// <summary>
/// A concise view of an Amazon API Gateway v2 API.
/// </summary>
/// <param name="ApiId">The unique identifier of the API.</param>
/// <param name="Name">The human-readable name of the API.</param>
/// <param name="ProtocolType">The protocol of the API (for example <c>HTTP</c> or <c>WEBSOCKET</c>).</param>
/// <param name="ApiEndpoint">The invoke endpoint of the API, or <see langword="null"/> when not reported.</param>
/// <param name="CreatedDate">The moment the API was created, or <see langword="null"/> when not reported.</param>
public sealed record HttpApiSummaryResponse(
    string ApiId,
    string Name,
    string ProtocolType,
    string? ApiEndpoint,
    DateTimeOffset? CreatedDate);

/// <summary>
/// The full configuration of an Amazon API Gateway v2 API.
/// </summary>
/// <param name="ApiId">The unique identifier of the API.</param>
/// <param name="Name">The human-readable name of the API.</param>
/// <param name="ProtocolType">The protocol of the API (for example <c>HTTP</c> or <c>WEBSOCKET</c>).</param>
/// <param name="ApiEndpoint">The invoke endpoint of the API, or <see langword="null"/> when not reported.</param>
/// <param name="Description">The description of the API, or <see langword="null"/> when not set.</param>
/// <param name="Version">The version identifier of the API, or <see langword="null"/> when not set.</param>
/// <param name="RouteSelectionExpression">The route selection expression of the API, or <see langword="null"/> when not set.</param>
/// <param name="CorsConfiguration">The CORS configuration of the API, or <see langword="null"/> when none is configured.</param>
/// <param name="CreatedDate">The moment the API was created, or <see langword="null"/> when not reported.</param>
public sealed record HttpApiDetailResponse(
    string ApiId,
    string Name,
    string ProtocolType,
    string? ApiEndpoint,
    string? Description,
    string? Version,
    string? RouteSelectionExpression,
    HttpApiCorsResponse? CorsConfiguration,
    DateTimeOffset? CreatedDate);

/// <summary>
/// The cross-origin resource sharing (CORS) configuration of an Amazon API Gateway v2 API.
/// </summary>
/// <param name="AllowCredentials">Whether credentials are allowed in CORS requests, or <see langword="null"/> when not reported.</param>
/// <param name="AllowHeaders">The headers that are allowed in CORS requests.</param>
/// <param name="AllowMethods">The HTTP methods that are allowed in CORS requests.</param>
/// <param name="AllowOrigins">The origins that are allowed to make CORS requests.</param>
/// <param name="ExposeHeaders">The headers that are exposed to the browser in CORS responses.</param>
/// <param name="MaxAge">The number of seconds a browser may cache the CORS preflight response, or <see langword="null"/> when not reported.</param>
public sealed record HttpApiCorsResponse(
    bool? AllowCredentials,
    IReadOnlyList<string> AllowHeaders,
    IReadOnlyList<string> AllowMethods,
    IReadOnlyList<string> AllowOrigins,
    IReadOnlyList<string> ExposeHeaders,
    int? MaxAge);

/// <summary>
/// A request to create an Amazon API Gateway v2 API.
/// </summary>
/// <param name="Name">The name of the API to create.</param>
/// <param name="ProtocolType">The protocol of the API (for example <c>HTTP</c> or <c>WEBSOCKET</c>).</param>
/// <param name="Description">The description of the API, or <see langword="null"/> for none.</param>
/// <param name="Version">The version identifier of the API, or <see langword="null"/> for none.</param>
/// <param name="RouteSelectionExpression">The route selection expression of the API, or <see langword="null"/> to use the backend default.</param>
public sealed record HttpApiCreateRequest(
    string Name,
    string ProtocolType,
    string? Description,
    string? Version,
    string? RouteSelectionExpression);

/// <summary>
/// A request to update an Amazon API Gateway v2 API.
/// </summary>
/// <param name="Name">The name of the API.</param>
/// <param name="ProtocolType">The protocol of the API (for example <c>HTTP</c> or <c>WEBSOCKET</c>).</param>
/// <param name="Description">The description of the API, or <see langword="null"/> for none.</param>
/// <param name="Version">The version identifier of the API, or <see langword="null"/> for none.</param>
/// <param name="RouteSelectionExpression">The route selection expression of the API, or <see langword="null"/> to use the backend default.</param>
/// <param name="CorsConfiguration">The CORS configuration to apply, or <see langword="null"/> to leave the CORS configuration unchanged.</param>
public sealed record HttpApiUpdateRequest(
    string Name,
    string ProtocolType,
    string? Description,
    string? Version,
    string? RouteSelectionExpression,
    HttpApiCorsRequest? CorsConfiguration = null);

/// <summary>
/// The cross-origin resource sharing (CORS) configuration to apply to an Amazon API Gateway v2 API.
/// </summary>
/// <param name="AllowCredentials">Whether credentials are allowed in CORS requests, or <see langword="null"/> when not specified.</param>
/// <param name="AllowHeaders">The headers that are allowed in CORS requests.</param>
/// <param name="AllowMethods">The HTTP methods that are allowed in CORS requests.</param>
/// <param name="AllowOrigins">The origins that are allowed to make CORS requests.</param>
/// <param name="ExposeHeaders">The headers that are exposed to the browser in CORS responses.</param>
/// <param name="MaxAge">The number of seconds a browser may cache the CORS preflight response, or <see langword="null"/> when not specified.</param>
public sealed record HttpApiCorsRequest(
    bool? AllowCredentials,
    IReadOnlyList<string> AllowHeaders,
    IReadOnlyList<string> AllowMethods,
    IReadOnlyList<string> AllowOrigins,
    IReadOnlyList<string> ExposeHeaders,
    int? MaxAge);

/// <summary>
/// The result of creating an Amazon API Gateway v2 API.
/// </summary>
/// <param name="ApiId">The unique identifier of the created API.</param>
public sealed record HttpApiCreatedResponse(string ApiId);

/// <summary>
/// A request to invoke an Amazon API Gateway v2 route to verify its authorization behaviour.
/// </summary>
/// <param name="Stage">The stage to invoke (for example <c>$default</c> or a named stage).</param>
/// <param name="Method">The HTTP method to use (for example <c>GET</c> or <c>POST</c>).</param>
/// <param name="Path">The request path to invoke (for example <c>/items</c>).</param>
/// <param name="Token">An optional bearer token to send, or <see langword="null"/> to send an unauthenticated request.</param>
/// <param name="Body">An optional request body, or <see langword="null"/> to send no body.</param>
public sealed record HttpRouteTestRequest(
    string Stage,
    string Method,
    string Path,
    string? Token,
    string? Body);

/// <summary>
/// The outcome of invoking an Amazon API Gateway v2 route.
/// </summary>
/// <param name="StatusCode">The HTTP status code returned by the invocation.</param>
/// <param name="Authorized">Whether the request was authorized; <see langword="false"/> when the status code was 401 or 403.</param>
/// <param name="LatencyMilliseconds">The round-trip latency of the invocation in milliseconds.</param>
/// <param name="Headers">The response headers returned by the invocation.</param>
/// <param name="Body">The response body returned by the invocation.</param>
public sealed record HttpRouteInvocationResponse(
    int StatusCode,
    bool Authorized,
    long LatencyMilliseconds,
    IReadOnlyDictionary<string, string> Headers,
    string Body);

/// <summary>
/// A list of Amazon API Gateway v2 route summaries.
/// </summary>
/// <param name="Routes">The route summaries.</param>
public sealed record HttpRouteListResponse(IReadOnlyList<HttpRouteSummaryResponse> Routes);

/// <summary>
/// A concise view of an Amazon API Gateway v2 route.
/// </summary>
/// <param name="RouteId">The unique identifier of the route.</param>
/// <param name="RouteKey">The route key (for example <c>GET /items</c> or <c>$default</c>).</param>
/// <param name="Target">The integration target of the route, or <see langword="null"/> when not set.</param>
/// <param name="AuthorizationType">The authorization type of the route, or <see langword="null"/> when not reported.</param>
public sealed record HttpRouteSummaryResponse(
    string RouteId,
    string RouteKey,
    string? Target,
    string? AuthorizationType);

/// <summary>
/// The full configuration of an Amazon API Gateway v2 route.
/// </summary>
/// <param name="RouteId">The unique identifier of the route.</param>
/// <param name="RouteKey">The route key (for example <c>GET /items</c> or <c>$default</c>).</param>
/// <param name="Target">The integration target of the route, or <see langword="null"/> when not set.</param>
/// <param name="AuthorizationType">The authorization type of the route, or <see langword="null"/> when not reported.</param>
/// <param name="AuthorizerId">The identifier of the authorizer attached to the route, or <see langword="null"/> when none.</param>
/// <param name="AuthorizationScopes">The authorization scopes required by the route.</param>
/// <param name="ApiKeyRequired">Whether an API key is required for the route, or <see langword="null"/> when not reported.</param>
public sealed record HttpRouteDetailResponse(
    string RouteId,
    string RouteKey,
    string? Target,
    string? AuthorizationType,
    string? AuthorizerId,
    IReadOnlyList<string> AuthorizationScopes,
    bool? ApiKeyRequired);

/// <summary>
/// A request to create an Amazon API Gateway v2 route.
/// </summary>
/// <param name="RouteKey">The route key (for example <c>GET /items</c> or <c>$default</c>).</param>
/// <param name="Target">The integration target of the route, or <see langword="null"/> for none.</param>
/// <param name="AuthorizationType">The authorization type of the route, or <see langword="null"/> to use the backend default.</param>
/// <param name="AuthorizerId">The identifier of the authorizer to attach to the route, or <see langword="null"/> for none.</param>
/// <param name="AuthorizationScopes">The authorization scopes required by the route, or <see langword="null"/> for none.</param>
public sealed record HttpRouteCreateRequest(
    string RouteKey,
    string? Target,
    string? AuthorizationType,
    string? AuthorizerId,
    IReadOnlyList<string>? AuthorizationScopes);

/// <summary>
/// A request to update an Amazon API Gateway v2 route.
/// </summary>
/// <param name="RouteKey">The route key (for example <c>GET /items</c> or <c>$default</c>).</param>
/// <param name="Target">The integration target of the route, or <see langword="null"/> for none.</param>
/// <param name="AuthorizationType">The authorization type of the route, or <see langword="null"/> to use the backend default.</param>
/// <param name="AuthorizerId">The identifier of the authorizer to attach to the route, or <see langword="null"/> for none.</param>
/// <param name="AuthorizationScopes">The authorization scopes required by the route, or <see langword="null"/> for none.</param>
public sealed record HttpRouteUpdateRequest(
    string RouteKey,
    string? Target,
    string? AuthorizationType,
    string? AuthorizerId,
    IReadOnlyList<string>? AuthorizationScopes);

/// <summary>
/// The result of creating an Amazon API Gateway v2 route.
/// </summary>
/// <param name="RouteId">The unique identifier of the created route.</param>
public sealed record HttpRouteCreatedResponse(string RouteId);

/// <summary>
/// A list of Amazon API Gateway v2 integration summaries.
/// </summary>
/// <param name="Integrations">The integration summaries.</param>
public sealed record HttpIntegrationListResponse(IReadOnlyList<HttpIntegrationSummaryResponse> Integrations);

/// <summary>
/// A concise view of an Amazon API Gateway v2 integration.
/// </summary>
/// <param name="IntegrationId">The unique identifier of the integration.</param>
/// <param name="IntegrationType">The type of the integration (for example <c>HTTP_PROXY</c> or <c>MOCK</c>).</param>
/// <param name="IntegrationMethod">The HTTP method used to call the integration target, or <see langword="null"/> when not set.</param>
/// <param name="IntegrationUri">The URI of the integration target, or <see langword="null"/> when not set.</param>
/// <param name="PayloadFormatVersion">The payload format version of the integration, or <see langword="null"/> when not set.</param>
/// <param name="Description">The description of the integration, or <see langword="null"/> when not set.</param>
public sealed record HttpIntegrationSummaryResponse(
    string IntegrationId,
    string IntegrationType,
    string? IntegrationMethod,
    string? IntegrationUri,
    string? PayloadFormatVersion,
    string? Description);

/// <summary>
/// A request to create an Amazon API Gateway v2 integration.
/// </summary>
/// <param name="IntegrationType">The type of the integration (for example <c>HTTP_PROXY</c>, <c>AWS_PROXY</c>, or <c>MOCK</c>).</param>
/// <param name="IntegrationMethod">The HTTP method used to call the integration target, or <see langword="null"/> when not applicable.</param>
/// <param name="IntegrationUri">The URI of the integration target, or <see langword="null"/> when not applicable.</param>
/// <param name="PayloadFormatVersion">The payload format version of the integration, or <see langword="null"/> to use the backend default.</param>
/// <param name="Description">The description of the integration, or <see langword="null"/> for none.</param>
public sealed record HttpIntegrationCreateRequest(
    string IntegrationType,
    string? IntegrationMethod,
    string? IntegrationUri,
    string? PayloadFormatVersion,
    string? Description);

/// <summary>
/// A request to update an Amazon API Gateway v2 integration.
/// </summary>
/// <param name="IntegrationType">The type of the integration (for example <c>HTTP_PROXY</c>, <c>AWS_PROXY</c>, or <c>MOCK</c>).</param>
/// <param name="IntegrationMethod">The HTTP method used to call the integration target, or <see langword="null"/> when not applicable.</param>
/// <param name="IntegrationUri">The URI of the integration target, or <see langword="null"/> when not applicable.</param>
/// <param name="PayloadFormatVersion">The payload format version of the integration, or <see langword="null"/> to use the backend default.</param>
/// <param name="Description">The description of the integration, or <see langword="null"/> for none.</param>
public sealed record HttpIntegrationUpdateRequest(
    string IntegrationType,
    string? IntegrationMethod,
    string? IntegrationUri,
    string? PayloadFormatVersion,
    string? Description);

/// <summary>
/// The result of creating an Amazon API Gateway v2 integration.
/// </summary>
/// <param name="IntegrationId">The unique identifier of the created integration.</param>
public sealed record HttpIntegrationCreatedResponse(string IntegrationId);

/// <summary>
/// A list of Amazon API Gateway v2 authorizer summaries.
/// </summary>
/// <param name="Authorizers">The authorizer summaries.</param>
public sealed record HttpAuthorizerListResponse(IReadOnlyList<HttpAuthorizerSummaryResponse> Authorizers);

/// <summary>
/// A concise view of an Amazon API Gateway v2 authorizer.
/// </summary>
/// <param name="AuthorizerId">The unique identifier of the authorizer.</param>
/// <param name="Name">The human-readable name of the authorizer.</param>
/// <param name="AuthorizerType">The type of the authorizer (for example <c>JWT</c>).</param>
public sealed record HttpAuthorizerSummaryResponse(
    string AuthorizerId,
    string Name,
    string AuthorizerType);

/// <summary>
/// The full configuration of an Amazon API Gateway v2 authorizer.
/// </summary>
/// <param name="AuthorizerId">The unique identifier of the authorizer.</param>
/// <param name="Name">The human-readable name of the authorizer.</param>
/// <param name="AuthorizerType">The type of the authorizer (for example <c>JWT</c>).</param>
/// <param name="IdentitySource">The identity sources the authorizer reads the token from.</param>
/// <param name="JwtIssuer">The OpenID issuer URL for a JWT authorizer, or <see langword="null"/> when not applicable.</param>
/// <param name="JwtAudience">The allowed audiences for a JWT authorizer.</param>
public sealed record HttpAuthorizerDetailResponse(
    string AuthorizerId,
    string Name,
    string AuthorizerType,
    IReadOnlyList<string> IdentitySource,
    string? JwtIssuer,
    IReadOnlyList<string> JwtAudience);

/// <summary>
/// A request to create an Amazon API Gateway v2 authorizer.
/// </summary>
/// <param name="Name">The human-readable name of the authorizer.</param>
/// <param name="AuthorizerType">The type of the authorizer (for example <c>JWT</c>).</param>
/// <param name="IdentitySource">The identity sources the authorizer reads the token from (for example <c>$request.header.Authorization</c>).</param>
/// <param name="JwtIssuer">The OpenID issuer URL for a JWT authorizer, or <see langword="null"/> when not applicable.</param>
/// <param name="JwtAudience">The allowed audiences for a JWT authorizer (the Cognito app client identifiers).</param>
public sealed record HttpAuthorizerCreateRequest(
    string Name,
    string AuthorizerType,
    IReadOnlyList<string> IdentitySource,
    string? JwtIssuer,
    IReadOnlyList<string> JwtAudience);

/// <summary>
/// A request to update an Amazon API Gateway v2 authorizer.
/// </summary>
/// <param name="Name">The human-readable name of the authorizer.</param>
/// <param name="AuthorizerType">The type of the authorizer (for example <c>JWT</c>).</param>
/// <param name="IdentitySource">The identity sources the authorizer reads the token from (for example <c>$request.header.Authorization</c>).</param>
/// <param name="JwtIssuer">The OpenID issuer URL for a JWT authorizer, or <see langword="null"/> when not applicable.</param>
/// <param name="JwtAudience">The allowed audiences for a JWT authorizer (the Cognito app client identifiers).</param>
public sealed record HttpAuthorizerUpdateRequest(
    string Name,
    string AuthorizerType,
    IReadOnlyList<string> IdentitySource,
    string? JwtIssuer,
    IReadOnlyList<string> JwtAudience);

/// <summary>
/// The result of creating an Amazon API Gateway v2 authorizer.
/// </summary>
/// <param name="AuthorizerId">The unique identifier of the created authorizer.</param>
public sealed record HttpAuthorizerCreatedResponse(string AuthorizerId);

/// <summary>
/// A list of Amazon API Gateway v2 stage summaries.
/// </summary>
/// <param name="Stages">The stage summaries.</param>
public sealed record HttpStageListResponse(IReadOnlyList<HttpStageSummaryResponse> Stages);

/// <summary>
/// A concise view of an Amazon API Gateway v2 stage.
/// </summary>
/// <param name="StageName">The name of the stage.</param>
/// <param name="AutoDeploy">Whether updates to the API are automatically deployed to the stage.</param>
/// <param name="DeploymentId">The identifier of the deployment currently associated with the stage, or <see langword="null"/> when none.</param>
/// <param name="CreatedDate">The time the stage was created, or <see langword="null"/> when unknown.</param>
public sealed record HttpStageSummaryResponse(
    string StageName,
    bool AutoDeploy,
    string? DeploymentId,
    DateTimeOffset? CreatedDate);

/// <summary>
/// The full configuration of an Amazon API Gateway v2 stage.
/// </summary>
/// <param name="StageName">The name of the stage.</param>
/// <param name="AutoDeploy">Whether updates to the API are automatically deployed to the stage.</param>
/// <param name="DeploymentId">The identifier of the deployment currently associated with the stage, or <see langword="null"/> when none.</param>
/// <param name="Description">A human-readable description of the stage, or <see langword="null"/> when none.</param>
/// <param name="DefaultRouteThrottlingBurstLimit">The default route throttling burst limit, or <see langword="null"/> when not configured.</param>
/// <param name="DefaultRouteThrottlingRateLimit">The default route throttling rate limit, or <see langword="null"/> when not configured.</param>
/// <param name="StageVariables">The stage variables configured for the stage.</param>
/// <param name="CreatedDate">The time the stage was created, or <see langword="null"/> when unknown.</param>
/// <param name="LastUpdatedDate">The time the stage was last updated, or <see langword="null"/> when unknown.</param>
public sealed record HttpStageDetailResponse(
    string StageName,
    bool AutoDeploy,
    string? DeploymentId,
    string? Description,
    int? DefaultRouteThrottlingBurstLimit,
    double? DefaultRouteThrottlingRateLimit,
    IReadOnlyDictionary<string, string> StageVariables,
    DateTimeOffset? CreatedDate,
    DateTimeOffset? LastUpdatedDate);

/// <summary>
/// A request to create an Amazon API Gateway v2 stage.
/// </summary>
/// <param name="StageName">The name of the stage (for example <c>$default</c>, <c>dev</c> or <c>prod</c>).</param>
/// <param name="AutoDeploy">Whether updates to the API are automatically deployed to the stage.</param>
/// <param name="Description">A human-readable description of the stage, or <see langword="null"/> when none.</param>
/// <param name="DefaultRouteThrottlingBurstLimit">The default route throttling burst limit, or <see langword="null"/> when not configured.</param>
/// <param name="DefaultRouteThrottlingRateLimit">The default route throttling rate limit, or <see langword="null"/> when not configured.</param>
/// <param name="StageVariables">The stage variables to configure on the stage.</param>
public sealed record HttpStageCreateRequest(
    string StageName,
    bool AutoDeploy,
    string? Description,
    int? DefaultRouteThrottlingBurstLimit,
    double? DefaultRouteThrottlingRateLimit,
    IReadOnlyDictionary<string, string> StageVariables);

/// <summary>
/// A request to update an Amazon API Gateway v2 stage.
/// </summary>
/// <param name="AutoDeploy">Whether updates to the API are automatically deployed to the stage.</param>
/// <param name="Description">A human-readable description of the stage, or <see langword="null"/> when none.</param>
/// <param name="DefaultRouteThrottlingBurstLimit">The default route throttling burst limit, or <see langword="null"/> when not configured.</param>
/// <param name="DefaultRouteThrottlingRateLimit">The default route throttling rate limit, or <see langword="null"/> when not configured.</param>
/// <param name="StageVariables">The stage variables to configure on the stage.</param>
public sealed record HttpStageUpdateRequest(
    bool AutoDeploy,
    string? Description,
    int? DefaultRouteThrottlingBurstLimit,
    double? DefaultRouteThrottlingRateLimit,
    IReadOnlyDictionary<string, string> StageVariables);

/// <summary>
/// The result of creating an Amazon API Gateway v2 stage.
/// </summary>
/// <param name="StageName">The name of the created stage.</param>
public sealed record HttpStageCreatedResponse(string StageName);
