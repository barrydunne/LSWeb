using System.Diagnostics.CodeAnalysis;
using Amazon.ApiGatewayV2;
using Amazon.ApiGatewayV2.Model;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGatewayV2;
using Foundation.Domain.ApiGatewayV2;
using Foundation.Infrastructure.Aws;

namespace Foundation.Infrastructure.ApiGatewayV2;

/// <summary>
/// Reads and manages Amazon API Gateway v2 APIs through the resilient AWS gateway so the same code
/// works against LocalStack or real AWS. All access flows through <see cref="IAwsGateway"/>, which
/// records capability and converts failures into a <see cref="Result{T}"/> rather than throwing.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
internal sealed class ApiGatewayV2ClientAdapter : IApiGatewayV2Client
{
    private const string ServiceKey = "apigatewayv2";
    private const string PageSize = "100";

    private readonly IAwsGateway _gateway;

    public ApiGatewayV2ClientAdapter(IAwsGateway gateway)
        => _gateway = gateway;

    public Task<Result<IReadOnlyList<HttpApiSummary>>> ListApisAsync(
        CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonApiGatewayV2Client, IReadOnlyList<HttpApiSummary>>(
            ServiceKey,
            async (client, token) =>
            {
                var apis = new List<HttpApiSummary>();
                string? nextToken = null;

                do
                {
                    var response = await client.GetApisAsync(
                        new GetApisRequest { MaxResults = PageSize, NextToken = nextToken },
                        token);

                    foreach (var api in response.Items ?? [])
                        apis.Add(ToSummary(api));

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return apis;
            },
            cancellationToken);

    public Task<Result<HttpApiDetail>> GetApiAsync(
        string apiId, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonApiGatewayV2Client, HttpApiDetail>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.GetApiAsync(
                    new GetApiRequest { ApiId = apiId },
                    token);

                return ToDetail(response);
            },
            cancellationToken);

    public async Task<Result<string>> CreateApiAsync(
        HttpApiSpecification specification, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonApiGatewayV2Client, string>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new CreateApiRequest
                {
                    Name = specification.Name,
                    ProtocolType = ProtocolType.FindValue(specification.ProtocolType),
                    Description = specification.Description,
                    Version = specification.Version,
                    RouteSelectionExpression = specification.RouteSelectionExpression,
                };

                var response = await client.CreateApiAsync(request, token);
                return response.ApiId ?? string.Empty;
            },
            cancellationToken);

        return result;
    }

    public async Task<Result> UpdateApiAsync(
        HttpApiSpecification specification, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonApiGatewayV2Client, bool>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new UpdateApiRequest
                {
                    ApiId = specification.ApiId,
                    Name = specification.Name,
                    Description = specification.Description,
                    Version = specification.Version,
                    RouteSelectionExpression = specification.RouteSelectionExpression,
                };

                if (specification.CorsConfiguration is not null)
                    request.CorsConfiguration = FromCors(specification.CorsConfiguration);

                await client.UpdateApiAsync(request, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteApiAsync(
        string apiId, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonApiGatewayV2Client, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteApiAsync(
                    new DeleteApiRequest { ApiId = apiId },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<IReadOnlyList<HttpRouteSummary>>> ListRoutesAsync(
        string apiId, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonApiGatewayV2Client, IReadOnlyList<HttpRouteSummary>>(
            ServiceKey,
            async (client, token) =>
            {
                var routes = new List<HttpRouteSummary>();
                string? nextToken = null;

                do
                {
                    var response = await client.GetRoutesAsync(
                        new GetRoutesRequest { ApiId = apiId, MaxResults = PageSize, NextToken = nextToken },
                        token);

                    foreach (var route in response.Items ?? [])
                        routes.Add(ToRouteSummary(route));

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return routes;
            },
            cancellationToken);

    public Task<Result<HttpRouteDetail>> GetRouteAsync(
        string apiId, string routeId, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonApiGatewayV2Client, HttpRouteDetail>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.GetRouteAsync(
                    new GetRouteRequest { ApiId = apiId, RouteId = routeId },
                    token);

                return ToRouteDetail(response);
            },
            cancellationToken);

    public async Task<Result<string>> CreateRouteAsync(
        HttpRouteSpecification specification, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonApiGatewayV2Client, string>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new CreateRouteRequest
                {
                    ApiId = specification.ApiId,
                    RouteKey = specification.RouteKey,
                    Target = specification.Target,
                    AuthorizationType = string.IsNullOrEmpty(specification.AuthorizationType)
                        ? null
                        : AuthorizationType.FindValue(specification.AuthorizationType),
                    AuthorizerId = specification.AuthorizerId,
                    AuthorizationScopes = specification.AuthorizationScopes.Count > 0
                        ? specification.AuthorizationScopes.ToList()
                        : null,
                };

                var response = await client.CreateRouteAsync(request, token);
                return response.RouteId ?? string.Empty;
            },
            cancellationToken);

        return result;
    }

    public async Task<Result> UpdateRouteAsync(
        HttpRouteSpecification specification, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonApiGatewayV2Client, bool>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new UpdateRouteRequest
                {
                    ApiId = specification.ApiId,
                    RouteId = specification.RouteId,
                    RouteKey = specification.RouteKey,
                    Target = specification.Target,
                    AuthorizationType = string.IsNullOrEmpty(specification.AuthorizationType)
                        ? null
                        : AuthorizationType.FindValue(specification.AuthorizationType),
                    AuthorizerId = specification.AuthorizerId,
                    AuthorizationScopes = specification.AuthorizationScopes.Count > 0
                        ? specification.AuthorizationScopes.ToList()
                        : null,
                };

                await client.UpdateRouteAsync(request, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteRouteAsync(
        string apiId, string routeId, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonApiGatewayV2Client, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteRouteAsync(
                    new DeleteRouteRequest { ApiId = apiId, RouteId = routeId },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<IReadOnlyList<HttpIntegrationSummary>>> ListIntegrationsAsync(
        string apiId, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonApiGatewayV2Client, IReadOnlyList<HttpIntegrationSummary>>(
            ServiceKey,
            async (client, token) =>
            {
                var integrations = new List<HttpIntegrationSummary>();
                string? nextToken = null;

                do
                {
                    var response = await client.GetIntegrationsAsync(
                        new GetIntegrationsRequest { ApiId = apiId, MaxResults = PageSize, NextToken = nextToken },
                        token);

                    foreach (var integration in response.Items ?? [])
                        integrations.Add(ToIntegrationSummary(integration));

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return integrations;
            },
            cancellationToken);

    public async Task<Result<string>> CreateIntegrationAsync(
        HttpIntegrationSpecification specification, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonApiGatewayV2Client, string>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new CreateIntegrationRequest
                {
                    ApiId = specification.ApiId,
                    IntegrationType = IntegrationType.FindValue(specification.IntegrationType),
                    IntegrationMethod = specification.IntegrationMethod,
                    IntegrationUri = specification.IntegrationUri,
                    PayloadFormatVersion = specification.PayloadFormatVersion,
                    Description = specification.Description,
                };

                var response = await client.CreateIntegrationAsync(request, token);
                return response.IntegrationId ?? string.Empty;
            },
            cancellationToken);

        return result;
    }

    public async Task<Result> UpdateIntegrationAsync(
        HttpIntegrationSpecification specification, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonApiGatewayV2Client, bool>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new UpdateIntegrationRequest
                {
                    ApiId = specification.ApiId,
                    IntegrationId = specification.IntegrationId,
                    IntegrationType = IntegrationType.FindValue(specification.IntegrationType),
                    IntegrationMethod = specification.IntegrationMethod,
                    IntegrationUri = specification.IntegrationUri,
                    PayloadFormatVersion = specification.PayloadFormatVersion,
                    Description = specification.Description,
                };

                await client.UpdateIntegrationAsync(request, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteIntegrationAsync(
        string apiId, string integrationId, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonApiGatewayV2Client, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteIntegrationAsync(
                    new DeleteIntegrationRequest { ApiId = apiId, IntegrationId = integrationId },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<IReadOnlyList<HttpAuthorizerSummary>>> ListAuthorizersAsync(
        string apiId, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonApiGatewayV2Client, IReadOnlyList<HttpAuthorizerSummary>>(
            ServiceKey,
            async (client, token) =>
            {
                var authorizers = new List<HttpAuthorizerSummary>();
                string? nextToken = null;

                do
                {
                    var response = await client.GetAuthorizersAsync(
                        new GetAuthorizersRequest { ApiId = apiId, MaxResults = PageSize, NextToken = nextToken },
                        token);

                    foreach (var authorizer in response.Items ?? [])
                        authorizers.Add(ToAuthorizerSummary(authorizer));

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return authorizers;
            },
            cancellationToken);

    public Task<Result<HttpAuthorizerDetail>> GetAuthorizerAsync(
        string apiId, string authorizerId, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonApiGatewayV2Client, HttpAuthorizerDetail>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.GetAuthorizerAsync(
                    new GetAuthorizerRequest { ApiId = apiId, AuthorizerId = authorizerId },
                    token);

                return ToAuthorizerDetail(response);
            },
            cancellationToken);

    public async Task<Result<string>> CreateAuthorizerAsync(
        HttpAuthorizerSpecification specification, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonApiGatewayV2Client, string>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new CreateAuthorizerRequest
                {
                    ApiId = specification.ApiId,
                    Name = specification.Name,
                    AuthorizerType = AuthorizerType.FindValue(specification.AuthorizerType),
                    IdentitySource = specification.IdentitySource.ToList(),
                    JwtConfiguration = new JWTConfiguration
                    {
                        Issuer = specification.JwtIssuer,
                        Audience = specification.JwtAudience.ToList(),
                    },
                };

                var response = await client.CreateAuthorizerAsync(request, token);
                return response.AuthorizerId ?? string.Empty;
            },
            cancellationToken);

        return result;
    }

    public async Task<Result> UpdateAuthorizerAsync(
        HttpAuthorizerSpecification specification, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonApiGatewayV2Client, bool>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new UpdateAuthorizerRequest
                {
                    ApiId = specification.ApiId,
                    AuthorizerId = specification.AuthorizerId,
                    Name = specification.Name,
                    AuthorizerType = AuthorizerType.FindValue(specification.AuthorizerType),
                    IdentitySource = specification.IdentitySource.ToList(),
                    JwtConfiguration = new JWTConfiguration
                    {
                        Issuer = specification.JwtIssuer,
                        Audience = specification.JwtAudience.ToList(),
                    },
                };

                await client.UpdateAuthorizerAsync(request, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteAuthorizerAsync(
        string apiId, string authorizerId, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonApiGatewayV2Client, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteAuthorizerAsync(
                    new DeleteAuthorizerRequest { ApiId = apiId, AuthorizerId = authorizerId },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<IReadOnlyList<HttpStageSummary>>> ListStagesAsync(
        string apiId, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonApiGatewayV2Client, IReadOnlyList<HttpStageSummary>>(
            ServiceKey,
            async (client, token) =>
            {
                var stages = new List<HttpStageSummary>();
                string? nextToken = null;

                do
                {
                    var response = await client.GetStagesAsync(
                        new GetStagesRequest { ApiId = apiId, MaxResults = PageSize, NextToken = nextToken },
                        token);

                    foreach (var stage in response.Items ?? [])
                        stages.Add(ToStageSummary(stage));

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return stages;
            },
            cancellationToken);

    public Task<Result<HttpStageDetail>> GetStageAsync(
        string apiId, string stageName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonApiGatewayV2Client, HttpStageDetail>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.GetStageAsync(
                    new GetStageRequest { ApiId = apiId, StageName = stageName },
                    token);

                return ToStageDetail(response);
            },
            cancellationToken);

    public async Task<Result<string>> CreateStageAsync(
        HttpStageSpecification specification, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonApiGatewayV2Client, string>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new CreateStageRequest
                {
                    ApiId = specification.ApiId,
                    StageName = specification.StageName,
                    AutoDeploy = specification.AutoDeploy,
                    Description = specification.Description,
                    DefaultRouteSettings = ToRouteSettings(specification),
                    StageVariables = specification.StageVariables.Count == 0
                        ? null
                        : new Dictionary<string, string>(specification.StageVariables),
                };

                var response = await client.CreateStageAsync(request, token);
                return response.StageName ?? string.Empty;
            },
            cancellationToken);

        return result;
    }

    public async Task<Result> UpdateStageAsync(
        HttpStageSpecification specification, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonApiGatewayV2Client, bool>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new UpdateStageRequest
                {
                    ApiId = specification.ApiId,
                    StageName = specification.StageName,
                    AutoDeploy = specification.AutoDeploy,
                    Description = specification.Description,
                    DefaultRouteSettings = ToRouteSettings(specification),
                    StageVariables = specification.StageVariables.Count == 0
                        ? null
                        : new Dictionary<string, string>(specification.StageVariables),
                };

                await client.UpdateStageAsync(request, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteStageAsync(
        string apiId, string stageName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonApiGatewayV2Client, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteStageAsync(
                    new DeleteStageRequest { ApiId = apiId, StageName = stageName },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    private static RouteSettings? ToRouteSettings(HttpStageSpecification specification)
    {
        if (specification.DefaultRouteThrottlingBurstLimit is null
            && specification.DefaultRouteThrottlingRateLimit is null)
            return null;

        return new RouteSettings
        {
            ThrottlingBurstLimit = specification.DefaultRouteThrottlingBurstLimit,
            ThrottlingRateLimit = specification.DefaultRouteThrottlingRateLimit,
        };
    }

    private static HttpStageSummary ToStageSummary(Stage stage)
        => new(
            stage.StageName ?? string.Empty,
            stage.AutoDeploy ?? false,
            string.IsNullOrEmpty(stage.DeploymentId) ? null : stage.DeploymentId,
            ToTimestamp(stage.CreatedDate));

    private static HttpStageDetail ToStageDetail(GetStageResponse response)
        => new(
            response.StageName ?? string.Empty,
            response.AutoDeploy ?? false,
            string.IsNullOrEmpty(response.DeploymentId) ? null : response.DeploymentId,
            string.IsNullOrEmpty(response.Description) ? null : response.Description,
            response.DefaultRouteSettings?.ThrottlingBurstLimit,
            response.DefaultRouteSettings?.ThrottlingRateLimit,
            response.StageVariables is null
                ? new Dictionary<string, string>()
                : new Dictionary<string, string>(response.StageVariables),
            ToTimestamp(response.CreatedDate),
            ToTimestamp(response.LastUpdatedDate));

    private static HttpAuthorizerSummary ToAuthorizerSummary(Authorizer authorizer)
        => new(
            authorizer.AuthorizerId ?? string.Empty,
            authorizer.Name ?? string.Empty,
            authorizer.AuthorizerType?.Value ?? string.Empty);

    private static HttpAuthorizerDetail ToAuthorizerDetail(GetAuthorizerResponse response)
        => new(
            response.AuthorizerId ?? string.Empty,
            response.Name ?? string.Empty,
            response.AuthorizerType?.Value ?? string.Empty,
            (response.IdentitySource ?? []).ToList(),
            string.IsNullOrEmpty(response.JwtConfiguration?.Issuer) ? null : response.JwtConfiguration.Issuer,
            (response.JwtConfiguration?.Audience ?? []).ToList());

    private static HttpRouteSummary ToRouteSummary(Route route)
        => new(
            route.RouteId ?? string.Empty,
            route.RouteKey ?? string.Empty,
            string.IsNullOrEmpty(route.Target) ? null : route.Target,
            route.AuthorizationType?.Value);

    private static HttpRouteDetail ToRouteDetail(GetRouteResponse response)
        => new(
            response.RouteId ?? string.Empty,
            response.RouteKey ?? string.Empty,
            string.IsNullOrEmpty(response.Target) ? null : response.Target,
            response.AuthorizationType?.Value,
            string.IsNullOrEmpty(response.AuthorizerId) ? null : response.AuthorizerId,
            (response.AuthorizationScopes ?? []).ToList(),
            response.ApiKeyRequired);

    private static HttpIntegrationSummary ToIntegrationSummary(Integration integration)
        => new(
            integration.IntegrationId ?? string.Empty,
            integration.IntegrationType?.Value ?? string.Empty,
            string.IsNullOrEmpty(integration.IntegrationMethod) ? null : integration.IntegrationMethod,
            string.IsNullOrEmpty(integration.IntegrationUri) ? null : integration.IntegrationUri,
            string.IsNullOrEmpty(integration.PayloadFormatVersion) ? null : integration.PayloadFormatVersion,
            string.IsNullOrEmpty(integration.Description) ? null : integration.Description);

    private static HttpApiSummary ToSummary(Api api)
        => new(
            api.ApiId ?? string.Empty,
            api.Name ?? string.Empty,
            api.ProtocolType?.Value ?? string.Empty,
            string.IsNullOrEmpty(api.ApiEndpoint) ? null : api.ApiEndpoint,
            ToTimestamp(api.CreatedDate));

    private static HttpApiDetail ToDetail(GetApiResponse response)
        => new(
            response.ApiId ?? string.Empty,
            response.Name ?? string.Empty,
            response.ProtocolType?.Value ?? string.Empty,
            string.IsNullOrEmpty(response.ApiEndpoint) ? null : response.ApiEndpoint,
            string.IsNullOrEmpty(response.Description) ? null : response.Description,
            string.IsNullOrEmpty(response.Version) ? null : response.Version,
            string.IsNullOrEmpty(response.RouteSelectionExpression) ? null : response.RouteSelectionExpression,
            ToCors(response.CorsConfiguration),
            ToTimestamp(response.CreatedDate));

    private static HttpApiCorsConfiguration? ToCors(Cors? cors)
    {
        if (cors is null)
            return null;

        return new HttpApiCorsConfiguration(
            cors.AllowCredentials,
            (cors.AllowHeaders ?? []).ToList(),
            (cors.AllowMethods ?? []).ToList(),
            (cors.AllowOrigins ?? []).ToList(),
            (cors.ExposeHeaders ?? []).ToList(),
            cors.MaxAge);
    }

    private static Cors FromCors(HttpApiCorsConfiguration cors)
        => new()
        {
            AllowCredentials = cors.AllowCredentials,
            AllowHeaders = cors.AllowHeaders.ToList(),
            AllowMethods = cors.AllowMethods.ToList(),
            AllowOrigins = cors.AllowOrigins.ToList(),
            ExposeHeaders = cors.ExposeHeaders.ToList(),
            MaxAge = cors.MaxAge,
        };

    private static DateTimeOffset? ToTimestamp(DateTime? value)
        => value is null
            ? null
            : new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc), TimeSpan.Zero);
}
