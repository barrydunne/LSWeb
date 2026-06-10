using System.Diagnostics.CodeAnalysis;
using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGateway;
using Foundation.Domain.ApiGateway;
using Foundation.Infrastructure.Aws;
using DomainRestApi = Foundation.Domain.ApiGateway.RestApi;
using SdkRestApi = Amazon.APIGateway.Model.RestApi;

namespace Foundation.Infrastructure.ApiGateway;

/// <summary>
/// Reads API Gateway through the resilient AWS gateway so the same code works against LocalStack or
/// real AWS. All access flows through <see cref="IAwsGateway"/>, which records capability and
/// converts failures into a <see cref="Result{T}"/> rather than throwing.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
internal sealed class ApiGatewayClientAdapter : IApiGatewayClient
{
    private const string ServiceKey = "apigateway";

    private readonly IAwsGateway _gateway;

    public ApiGatewayClientAdapter(IAwsGateway gateway)
        => _gateway = gateway;

    public Task<Result<IReadOnlyList<DomainRestApi>>> ListRestApisAsync(
        CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonAPIGatewayClient, IReadOnlyList<DomainRestApi>>(
            ServiceKey,
            async (client, token) =>
            {
                var restApis = new List<DomainRestApi>();
                string? position = null;

                do
                {
                    var response = await client.GetRestApisAsync(
                        new GetRestApisRequest { Position = position },
                        token);

                    foreach (var item in response.Items ?? [])
                        restApis.Add(ToRestApi(item));

                    position = response.Position;
                }
                while (!string.IsNullOrEmpty(position));

                return restApis;
            },
            cancellationToken);

    public Task<Result<RestApiDetail>> GetRestApiAsync(
        string restApiId, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonAPIGatewayClient, RestApiDetail>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.GetRestApiAsync(
                    new GetRestApiRequest { RestApiId = restApiId },
                    token);

                return ToDetail(response);
            },
            cancellationToken);

    public Task<Result<string>> CreateRestApiAsync(
        RestApiSpecification specification, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonAPIGatewayClient, string>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new CreateRestApiRequest
                {
                    Name = specification.Name,
                    Description = specification.Description,
                    Version = specification.Version,
                    ApiKeySource = string.IsNullOrEmpty(specification.ApiKeySource)
                        ? null
                        : ApiKeySourceType.FindValue(specification.ApiKeySource),
                };

                if (specification.EndpointConfigurationTypes.Count > 0)
                {
                    request.EndpointConfiguration = new EndpointConfiguration
                    {
                        Types = [.. specification.EndpointConfigurationTypes],
                    };
                }

                var response = await client.CreateRestApiAsync(request, token);
                return response.Id ?? string.Empty;
            },
            cancellationToken);

    public async Task<Result> UpdateRestApiAsync(
        RestApiSpecification specification, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonAPIGatewayClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                var patchOperations = new List<PatchOperation>
                {
                    new() { Op = Op.Replace, Path = "/name", Value = specification.Name },
                    new()
                    {
                        Op = Op.Replace,
                        Path = "/description",
                        Value = specification.Description ?? string.Empty,
                    },
                };

                await client.UpdateRestApiAsync(
                    new UpdateRestApiRequest
                    {
                        RestApiId = specification.Id,
                        PatchOperations = patchOperations,
                    },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteRestApiAsync(
        string restApiId, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonAPIGatewayClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteRestApiAsync(
                    new DeleteRestApiRequest { RestApiId = restApiId },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<IReadOnlyList<RestResourceSummary>>> ListResourcesAsync(
        string restApiId, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonAPIGatewayClient, IReadOnlyList<RestResourceSummary>>(
            ServiceKey,
            async (client, token) =>
            {
                var resources = new List<RestResourceSummary>();
                string? position = null;

                do
                {
                    var response = await client.GetResourcesAsync(
                        new GetResourcesRequest { RestApiId = restApiId, Position = position },
                        token);

                    foreach (var item in response.Items ?? [])
                        resources.Add(ToResourceSummary(item));

                    position = response.Position;
                }
                while (!string.IsNullOrEmpty(position));

                return resources;
            },
            cancellationToken);

    public Task<Result<string>> CreateResourceAsync(
        RestResourceSpecification specification, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonAPIGatewayClient, string>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.CreateResourceAsync(
                    new CreateResourceRequest
                    {
                        RestApiId = specification.RestApiId,
                        ParentId = specification.ParentId,
                        PathPart = specification.PathPart,
                    },
                    token);

                return response.Id ?? string.Empty;
            },
            cancellationToken);

    public async Task<Result> DeleteResourceAsync(
        string restApiId, string resourceId, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonAPIGatewayClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteResourceAsync(
                    new DeleteResourceRequest { RestApiId = restApiId, ResourceId = resourceId },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<RestMethodDetail>> GetMethodAsync(
        string restApiId, string resourceId, string httpMethod, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonAPIGatewayClient, RestMethodDetail>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.GetMethodAsync(
                    new GetMethodRequest
                    {
                        RestApiId = restApiId,
                        ResourceId = resourceId,
                        HttpMethod = httpMethod,
                    },
                    token);

                GetIntegrationResponse? integrationResponse = null;
                try
                {
                    integrationResponse = await client.GetIntegrationAsync(
                        new GetIntegrationRequest
                        {
                            RestApiId = restApiId,
                            ResourceId = resourceId,
                            HttpMethod = httpMethod,
                        },
                        token);
                }
                catch (NotFoundException)
                {
                }

                return ToMethodDetail(resourceId, response, integrationResponse);
            },
            cancellationToken);

    public async Task<Result> PutMethodAsync(
        RestMethodSpecification specification, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonAPIGatewayClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.PutMethodAsync(
                    new PutMethodRequest
                    {
                        RestApiId = specification.RestApiId,
                        ResourceId = specification.ResourceId,
                        HttpMethod = specification.HttpMethod,
                        AuthorizationType = specification.AuthorizationType,
                        AuthorizerId = string.IsNullOrEmpty(specification.AuthorizerId)
                            ? null
                            : specification.AuthorizerId,
                        ApiKeyRequired = specification.ApiKeyRequired,
                        AuthorizationScopes = specification.AuthorizationScopes.Count > 0
                            ? [.. specification.AuthorizationScopes]
                            : null,
                    },
                    token);

                await client.PutIntegrationAsync(
                    new PutIntegrationRequest
                    {
                        RestApiId = specification.RestApiId,
                        ResourceId = specification.ResourceId,
                        HttpMethod = specification.HttpMethod,
                        Type = IntegrationType.FindValue(specification.IntegrationType),
                        Uri = string.IsNullOrWhiteSpace(specification.IntegrationUri)
                            ? null
                            : specification.IntegrationUri,
                        IntegrationHttpMethod = ResolveIntegrationHttpMethod(specification),
                        RequestTemplates = specification.IntegrationType == IntegrationType.MOCK.Value
                            ? new Dictionary<string, string>
                            {
                                ["application/json"] = "{\"statusCode\": 200}",
                            }
                            : null,
                    },
                    token);

                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteMethodAsync(
        string restApiId, string resourceId, string httpMethod, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonAPIGatewayClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteMethodAsync(
                    new DeleteMethodRequest
                    {
                        RestApiId = restApiId,
                        ResourceId = resourceId,
                        HttpMethod = httpMethod,
                    },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<RestCorsConfiguration>> GetCorsAsync(
        string restApiId, string resourceId, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonAPIGatewayClient, RestCorsConfiguration>(
            ServiceKey,
            async (client, token) =>
            {
                try
                {
                    var response = await client.GetIntegrationResponseAsync(
                        new GetIntegrationResponseRequest
                        {
                            RestApiId = restApiId,
                            ResourceId = resourceId,
                            HttpMethod = "OPTIONS",
                            StatusCode = "200",
                        },
                        token);

                    var parameters = response.ResponseParameters
                        ?? new Dictionary<string, string>();
                    return new RestCorsConfiguration(
                        resourceId,
                        true,
                        ReadCorsValues(parameters, "Access-Control-Allow-Origin"),
                        ReadCorsValues(parameters, "Access-Control-Allow-Methods"),
                        ReadCorsValues(parameters, "Access-Control-Allow-Headers"));
                }
                catch (NotFoundException)
                {
                    return new RestCorsConfiguration(resourceId, false, [], [], []);
                }
            },
            cancellationToken);

    public async Task<Result> ConfigureCorsAsync(
        RestCorsSpecification specification, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonAPIGatewayClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                try
                {
                    await client.DeleteMethodAsync(
                        new DeleteMethodRequest
                        {
                            RestApiId = specification.RestApiId,
                            ResourceId = specification.ResourceId,
                            HttpMethod = "OPTIONS",
                        },
                        token);
                }
                catch (NotFoundException)
                {
                }

                var allowOrigin = string.Join(",", specification.AllowOrigins);
                var allowMethods = string.Join(",", specification.AllowMethods);
                var allowHeaders = string.Join(",", specification.AllowHeaders);

                await client.PutMethodAsync(
                    new PutMethodRequest
                    {
                        RestApiId = specification.RestApiId,
                        ResourceId = specification.ResourceId,
                        HttpMethod = "OPTIONS",
                        AuthorizationType = "NONE",
                    },
                    token);

                await client.PutMethodResponseAsync(
                    new PutMethodResponseRequest
                    {
                        RestApiId = specification.RestApiId,
                        ResourceId = specification.ResourceId,
                        HttpMethod = "OPTIONS",
                        StatusCode = "200",
                        ResponseParameters = new Dictionary<string, bool>
                        {
                            ["method.response.header.Access-Control-Allow-Origin"] = false,
                            ["method.response.header.Access-Control-Allow-Methods"] = false,
                            ["method.response.header.Access-Control-Allow-Headers"] = false,
                        },
                    },
                    token);

                await client.PutIntegrationAsync(
                    new PutIntegrationRequest
                    {
                        RestApiId = specification.RestApiId,
                        ResourceId = specification.ResourceId,
                        HttpMethod = "OPTIONS",
                        Type = IntegrationType.MOCK,
                        RequestTemplates = new Dictionary<string, string>
                        {
                            ["application/json"] = "{\"statusCode\": 200}",
                        },
                    },
                    token);

                await client.PutIntegrationResponseAsync(
                    new PutIntegrationResponseRequest
                    {
                        RestApiId = specification.RestApiId,
                        ResourceId = specification.ResourceId,
                        HttpMethod = "OPTIONS",
                        StatusCode = "200",
                        ResponseParameters = new Dictionary<string, string>
                        {
                            ["method.response.header.Access-Control-Allow-Origin"] = $"'{allowOrigin}'",
                            ["method.response.header.Access-Control-Allow-Methods"] = $"'{allowMethods}'",
                            ["method.response.header.Access-Control-Allow-Headers"] = $"'{allowHeaders}'",
                        },
                    },
                    token);

                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    private static IReadOnlyList<string> ReadCorsValues(
        Dictionary<string, string> parameters, string headerName)
    {
        var key = $"method.response.header.{headerName}";
        if (!parameters.TryGetValue(key, out var raw) || string.IsNullOrWhiteSpace(raw))
            return [];

        var trimmed = raw.Trim().Trim('\'');
        return trimmed.Length == 0
            ? []
            : [.. trimmed.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
    }

    public Task<Result<RestMethodTestInvocationResult>> TestInvokeMethodAsync(
        RestMethodTestInvocationSpecification specification,
        CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonAPIGatewayClient, RestMethodTestInvocationResult>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.TestInvokeMethodAsync(
                    new TestInvokeMethodRequest
                    {
                        RestApiId = specification.RestApiId,
                        ResourceId = specification.ResourceId,
                        HttpMethod = specification.HttpMethod,
                        PathWithQueryString = BuildPathWithQueryString(specification),
                        Body = specification.Body,
                        Headers = specification.Headers.Count > 0
                            ? new Dictionary<string, string>(specification.Headers)
                            : null,
                        MultiValueHeaders = null,
                        StageVariables = specification.StageVariables.Count > 0
                            ? new Dictionary<string, string>(specification.StageVariables)
                            : null,
                    },
                    token);

                return new RestMethodTestInvocationResult(
                    response.Status ?? 0,
                    (int)(response.Latency ?? 0L),
                    response.Headers is { Count: > 0 } headers
                        ? new Dictionary<string, string>(headers)
                        : new Dictionary<string, string>(),
                    response.Body ?? string.Empty,
                    string.IsNullOrWhiteSpace(response.Log) ? null : response.Log);
            },
            cancellationToken);

    private static string BuildPathWithQueryString(RestMethodTestInvocationSpecification specification)
    {
        if (specification.QueryStringParameters.Count == 0)
            return specification.PathWithQueryString;

        var queryString = string.Join(
            '&',
            specification.QueryStringParameters.Select(pair =>
                $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}"));

        return specification.PathWithQueryString.Contains('?')
            ? $"{specification.PathWithQueryString}&{queryString}"
            : $"{specification.PathWithQueryString}?{queryString}";
    }

    public Task<Result<IReadOnlyList<RestAuthorizerSummary>>> ListAuthorizersAsync(
        string restApiId, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonAPIGatewayClient, IReadOnlyList<RestAuthorizerSummary>>(
            ServiceKey,
            async (client, token) =>
            {
                var authorizers = new List<RestAuthorizerSummary>();
                string? position = null;

                do
                {
                    var response = await client.GetAuthorizersAsync(
                        new GetAuthorizersRequest { RestApiId = restApiId, Position = position },
                        token);

                    foreach (var item in response.Items ?? [])
                        authorizers.Add(ToAuthorizerSummary(item));

                    position = response.Position;
                }
                while (!string.IsNullOrEmpty(position));

                return authorizers;
            },
            cancellationToken);

    public Task<Result<RestAuthorizerDetail>> GetAuthorizerAsync(
        string restApiId, string authorizerId, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonAPIGatewayClient, RestAuthorizerDetail>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.GetAuthorizerAsync(
                    new GetAuthorizerRequest { RestApiId = restApiId, AuthorizerId = authorizerId },
                    token);

                return ToAuthorizerDetail(response.Id, response.Name, response.Type,
                    response.ProviderARNs, response.IdentitySource, response.AuthType);
            },
            cancellationToken);

    public Task<Result<string>> CreateAuthorizerAsync(
        RestAuthorizerSpecification specification, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonAPIGatewayClient, string>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.CreateAuthorizerAsync(
                    new CreateAuthorizerRequest
                    {
                        RestApiId = specification.RestApiId,
                        Name = specification.Name,
                        Type = AuthorizerType.COGNITO_USER_POOLS,
                        ProviderARNs = [.. specification.ProviderARNs],
                        IdentitySource = string.IsNullOrEmpty(specification.IdentitySource)
                            ? "method.request.header.Authorization"
                            : specification.IdentitySource,
                    },
                    token);

                return response.Id ?? string.Empty;
            },
            cancellationToken);

    public Task<Result<string>> CreateTokenAuthorizerAsync(
        RestTokenAuthorizerSpecification specification, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonAPIGatewayClient, string>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.CreateAuthorizerAsync(
                    new CreateAuthorizerRequest
                    {
                        RestApiId = specification.RestApiId,
                        Name = specification.Name,
                        Type = AuthorizerType.TOKEN,
                        AuthType = "oauth2",
                        AuthorizerUri = specification.AuthorizerUri,
                        IdentitySource = specification.IdentitySource,
                        IdentityValidationExpression = "^Bearer [-0-9a-zA-Z._~+/]+=*$",
                    },
                    token);

                return response.Id ?? string.Empty;
            },
            cancellationToken);

    public async Task<Result> UpdateAuthorizerAsync(
        RestAuthorizerSpecification specification, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonAPIGatewayClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                var patchOperations = new List<PatchOperation>
                {
                    new() { Op = Op.Replace, Path = "/name", Value = specification.Name },
                    new()
                    {
                        Op = Op.Replace,
                        Path = "/providerARNs",
                        Value = string.Join(',', specification.ProviderARNs),
                    },
                };

                if (!string.IsNullOrEmpty(specification.IdentitySource))
                    patchOperations.Add(new PatchOperation
                    {
                        Op = Op.Replace,
                        Path = "/identitySource",
                        Value = specification.IdentitySource,
                    });

                await client.UpdateAuthorizerAsync(
                    new UpdateAuthorizerRequest
                    {
                        RestApiId = specification.RestApiId,
                        AuthorizerId = specification.AuthorizerId,
                        PatchOperations = patchOperations,
                    },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteAuthorizerAsync(
        string restApiId, string authorizerId, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonAPIGatewayClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteAuthorizerAsync(
                    new DeleteAuthorizerRequest { RestApiId = restApiId, AuthorizerId = authorizerId },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<IReadOnlyList<RestStageSummary>>> ListStagesAsync(
        string restApiId, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonAPIGatewayClient, IReadOnlyList<RestStageSummary>>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.GetStagesAsync(
                    new GetStagesRequest { RestApiId = restApiId }, token);

                return response.Item is { Count: > 0 } stages
                    ? [.. stages.Select(ToStageSummary)]
                    : (IReadOnlyList<RestStageSummary>)[];
            },
            cancellationToken);

    public Task<Result<RestStageDetail>> GetStageAsync(
        string restApiId, string stageName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonAPIGatewayClient, RestStageDetail>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.GetStageAsync(
                    new GetStageRequest { RestApiId = restApiId, StageName = stageName }, token);

                return ToStageDetail(response);
            },
            cancellationToken);

    public Task<Result<IReadOnlyList<RestDeploymentSummary>>> ListDeploymentsAsync(
        string restApiId, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonAPIGatewayClient, IReadOnlyList<RestDeploymentSummary>>(
            ServiceKey,
            async (client, token) =>
            {
                var deployments = new List<RestDeploymentSummary>();
                string? position = null;
                do
                {
                    var response = await client.GetDeploymentsAsync(
                        new GetDeploymentsRequest { RestApiId = restApiId, Position = position },
                        token);

                    if (response.Items is { Count: > 0 } items)
                        foreach (var item in items)
                            deployments.Add(ToDeploymentSummary(item));

                    position = response.Position;
                }
                while (!string.IsNullOrEmpty(position));

                return deployments;
            },
            cancellationToken);

    public Task<Result<string>> CreateDeploymentAsync(
        RestDeploymentSpecification specification, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonAPIGatewayClient, string>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.CreateDeploymentAsync(
                    new CreateDeploymentRequest
                    {
                        RestApiId = specification.RestApiId,
                        StageName = string.IsNullOrWhiteSpace(specification.StageName)
                            ? null
                            : specification.StageName,
                        Description = string.IsNullOrWhiteSpace(specification.Description)
                            ? null
                            : specification.Description,
                    },
                    token);

                return response.Id ?? string.Empty;
            },
            cancellationToken);

    public Task<Result<string>> CreateStageAsync(
        RestStageSpecification specification, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonAPIGatewayClient, string>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.CreateStageAsync(
                    new CreateStageRequest
                    {
                        RestApiId = specification.RestApiId,
                        StageName = specification.StageName,
                        DeploymentId = specification.DeploymentId,
                        Description = string.IsNullOrWhiteSpace(specification.Description)
                            ? null
                            : specification.Description,
                        Variables = specification.Variables.Count > 0
                            ? new Dictionary<string, string>(specification.Variables)
                            : null,
                    },
                    token);

                return response.StageName ?? specification.StageName;
            },
            cancellationToken);

    public async Task<Result> UpdateStageAsync(
        RestStageSpecification specification, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonAPIGatewayClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                var patchOperations = new List<PatchOperation>
                {
                    new()
                    {
                        Op = Op.Replace,
                        Path = "/description",
                        Value = specification.Description ?? string.Empty,
                    },
                };

                foreach (var variable in specification.Variables)
                    patchOperations.Add(new PatchOperation
                    {
                        Op = Op.Replace,
                        Path = $"/variables/{variable.Key}",
                        Value = variable.Value,
                    });

                await client.UpdateStageAsync(
                    new UpdateStageRequest
                    {
                        RestApiId = specification.RestApiId,
                        StageName = specification.StageName,
                        PatchOperations = patchOperations,
                    },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteStageAsync(
        string restApiId, string stageName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonAPIGatewayClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteStageAsync(
                    new DeleteStageRequest { RestApiId = restApiId, StageName = stageName },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    private static RestStageSummary ToStageSummary(Stage item)
        => new(
            item.StageName ?? string.Empty,
            item.DeploymentId ?? string.Empty,
            item.CreatedDate is { } created
                ? new DateTimeOffset(created.ToUniversalTime(), TimeSpan.Zero)
                : null);

    private static RestStageDetail ToStageDetail(GetStageResponse response)
        => new(
            response.StageName ?? string.Empty,
            response.DeploymentId ?? string.Empty,
            string.IsNullOrWhiteSpace(response.Description) ? null : response.Description,
            response.CacheClusterEnabled ?? false,
            response.Variables is { Count: > 0 } variables
                ? new Dictionary<string, string>(variables)
                : new Dictionary<string, string>(),
            response.CreatedDate is { } created
                ? new DateTimeOffset(created.ToUniversalTime(), TimeSpan.Zero)
                : null,
            response.LastUpdatedDate is { } updated
                ? new DateTimeOffset(updated.ToUniversalTime(), TimeSpan.Zero)
                : null);

    private static RestDeploymentSummary ToDeploymentSummary(Deployment item)
        => new(
            item.Id ?? string.Empty,
            string.IsNullOrWhiteSpace(item.Description) ? null : item.Description,
            item.CreatedDate is { } created
                ? new DateTimeOffset(created.ToUniversalTime(), TimeSpan.Zero)
                : null);

    private static RestAuthorizerSummary ToAuthorizerSummary(Authorizer item)
        => new(
            item.Id ?? string.Empty,
            item.Name ?? string.Empty,
            item.Type?.Value ?? string.Empty);

    private static RestAuthorizerDetail ToAuthorizerDetail(
        string? id, string? name, AuthorizerType? type,
        List<string>? providerArns, string? identitySource, string? authType)
        => new(
            id ?? string.Empty,
            name ?? string.Empty,
            type?.Value ?? string.Empty,
            providerArns ?? [],
            string.IsNullOrWhiteSpace(identitySource) ? null : identitySource,
            string.IsNullOrWhiteSpace(authType) ? null : authType);

    private static RestResourceSummary ToResourceSummary(Resource item)
        => new(
            item.Id ?? string.Empty,
            string.IsNullOrWhiteSpace(item.ParentId) ? null : item.ParentId,
            string.IsNullOrWhiteSpace(item.PathPart) ? null : item.PathPart,
            item.Path ?? string.Empty,
            item.ResourceMethods is { Count: > 0 } methods
                ? [.. methods.Keys]
                : []);

    private static RestMethodDetail ToMethodDetail(
        string resourceId,
        GetMethodResponse response,
        GetIntegrationResponse? integration)
        => new(
            resourceId,
            response.HttpMethod ?? string.Empty,
            string.IsNullOrWhiteSpace(response.AuthorizationType) ? "NONE" : response.AuthorizationType,
            string.IsNullOrWhiteSpace(response.AuthorizerId) ? null : response.AuthorizerId,
            response.ApiKeyRequired ?? false,
            response.AuthorizationScopes ?? [],
            integration?.Type?.Value ?? IntegrationType.MOCK.Value,
            string.IsNullOrWhiteSpace(integration?.Uri) ? null : integration.Uri);

    private static string? ResolveIntegrationHttpMethod(RestMethodSpecification specification)
    {
        if (specification.IntegrationType == IntegrationType.MOCK.Value)
            return null;

        if (specification.IntegrationType == IntegrationType.AWS.Value
            || specification.IntegrationType == IntegrationType.AWS_PROXY.Value)
            return "POST";

        if (specification.IntegrationType == IntegrationType.HTTP.Value
            || specification.IntegrationType == IntegrationType.HTTP_PROXY.Value)
            return specification.HttpMethod == "ANY" ? "GET" : specification.HttpMethod;

        return null;
    }

    private static DomainRestApi ToRestApi(SdkRestApi item)
        => new(
            item.Id ?? string.Empty,
            item.Name ?? string.Empty,
            string.IsNullOrWhiteSpace(item.Description) ? null : item.Description,
            item.CreatedDate is { } created
                ? new DateTimeOffset(created.ToUniversalTime(), TimeSpan.Zero)
                : null);

    private static RestApiDetail ToDetail(GetRestApiResponse response)
        => new(
            response.Id ?? string.Empty,
            response.Name ?? string.Empty,
            string.IsNullOrWhiteSpace(response.Description) ? null : response.Description,
            string.IsNullOrWhiteSpace(response.Version) ? null : response.Version,
            string.IsNullOrWhiteSpace(response.ApiKeySource) ? null : response.ApiKeySource,
            response.EndpointConfiguration?.Types?.ToList() ?? [],
            response.BinaryMediaTypes ?? [],
            response.CreatedDate is { } created
                ? new DateTimeOffset(created.ToUniversalTime(), TimeSpan.Zero)
                : null);
}
