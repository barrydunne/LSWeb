using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Lambda;
using Foundation.Domain.Lambda;
using Foundation.Infrastructure.Aws;

namespace Foundation.Infrastructure.Lambda;

/// <summary>
/// Reads Lambda function metadata through the resilient AWS gateway so the same code works against
/// LocalStack or real AWS. All access flows through <see cref="IAwsGateway"/>, which records
/// capability and converts failures into a <see cref="Result{T}"/> rather than throwing.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
internal sealed class LambdaClientAdapter : ILambdaClient
{
    private const string ServiceKey = "lambda";
    private const string LogsServiceKey = "cloudwatch-logs";

    private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(15) };

    private readonly IAwsGateway _gateway;

    public LambdaClientAdapter(IAwsGateway gateway)
        => _gateway = gateway;

    public Task<Result<IReadOnlyList<LambdaFunctionSummary>>> ListFunctionsAsync(CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonLambdaClient, IReadOnlyList<LambdaFunctionSummary>>(
            ServiceKey,
            async (client, token) =>
            {
                var summaries = new List<LambdaFunctionSummary>();
                string? marker = null;
                do
                {
                    var response = await client.ListFunctionsAsync(new ListFunctionsRequest { Marker = marker }, token);
                    foreach (var configuration in response.Functions ?? [])
                    {
                        summaries.Add(LambdaFunctionMapper.ToSummary(configuration));
                    }

                    marker = response.NextMarker;
                }
                while (!string.IsNullOrEmpty(marker));

                return summaries;
            },
            cancellationToken);

    public Task<Result<LambdaFunctionDetail>> GetFunctionAsync(string functionName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonLambdaClient, LambdaFunctionDetail>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.GetFunctionAsync(new GetFunctionRequest { FunctionName = functionName }, token);
                return LambdaFunctionMapper.ToDetail(response.Configuration);
            },
            cancellationToken);

    public Task<Result<LambdaFunctionCode>> GetFunctionCodeAsync(string functionName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonLambdaClient, LambdaFunctionCode>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.GetFunctionAsync(new GetFunctionRequest { FunctionName = functionName }, token);
                return LambdaFunctionMapper.ToCode(response.Configuration, response.Code);
            },
            cancellationToken);

    public Task<Result<LambdaFunctionUrl?>> GetFunctionUrlAsync(string functionName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonLambdaClient, LambdaFunctionUrl?>(
            ServiceKey,
            async (client, token) =>
            {
                try
                {
                    var response = await client.GetFunctionUrlConfigAsync(
                        new GetFunctionUrlConfigRequest { FunctionName = functionName }, token);
                    return LambdaFunctionMapper.ToFunctionUrl(
                        response.FunctionUrl, response.AuthType?.Value, response.CreationTime, response.LastModifiedTime);
                }
                catch (Amazon.Lambda.Model.ResourceNotFoundException)
                {
                    return null;
                }
            },
            cancellationToken);

    public Task<Result<LambdaFunctionUrl>> CreateFunctionUrlAsync(string functionName, string authType, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonLambdaClient, LambdaFunctionUrl>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.CreateFunctionUrlConfigAsync(
                    new CreateFunctionUrlConfigRequest
                    {
                        FunctionName = functionName,
                        AuthType = FunctionUrlAuthType.FindValue(authType),
                    },
                    token);
                return LambdaFunctionMapper.ToFunctionUrl(
                    response.FunctionUrl, response.AuthType?.Value, response.CreationTime, response.CreationTime);
            },
            cancellationToken);

    public Task<Result<LambdaFunctionUrl>> UpdateFunctionUrlAsync(string functionName, string authType, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonLambdaClient, LambdaFunctionUrl>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.UpdateFunctionUrlConfigAsync(
                    new UpdateFunctionUrlConfigRequest
                    {
                        FunctionName = functionName,
                        AuthType = FunctionUrlAuthType.FindValue(authType),
                    },
                    token);
                return LambdaFunctionMapper.ToFunctionUrl(
                    response.FunctionUrl, response.AuthType?.Value, response.CreationTime, response.LastModifiedTime);
            },
            cancellationToken);

    public async Task<Result> DeleteFunctionUrlAsync(string functionName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonLambdaClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteFunctionUrlConfigAsync(
                    new DeleteFunctionUrlConfigRequest { FunctionName = functionName }, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result<LambdaFunctionUrlTest>> TestFunctionUrlAsync(string functionName, CancellationToken cancellationToken)
    {
        var urlResult = await GetFunctionUrlAsync(functionName, cancellationToken);
        if (!urlResult.IsSuccess)
        {
            return urlResult.Error!.Value;
        }

        if (urlResult.Value is null || string.IsNullOrEmpty(urlResult.Value.FunctionUrl))
        {
            return new Error("No function URL is configured for this function.");
        }

        try
        {
            using var response = await _httpClient.GetAsync(urlResult.Value.FunctionUrl, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var truncated = body.Length > 4096 ? body[..4096] : body;
            return new LambdaFunctionUrlTest((int)response.StatusCode, truncated);
        }
        catch (HttpRequestException ex)
        {
            return new Error($"The function URL could not be reached: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return new Error("The function URL request timed out.");
        }
    }

    public Task<Result<IReadOnlyDictionary<string, string>>> GetEnvironmentAsync(string functionName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonLambdaClient, IReadOnlyDictionary<string, string>>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.GetFunctionConfigurationAsync(
                    new GetFunctionConfigurationRequest { FunctionName = functionName }, token);
                IReadOnlyDictionary<string, string> variables =
                    response.Environment?.Variables ?? new Dictionary<string, string>();
                return variables;
            },
            cancellationToken);

    public async Task<Result> UpdateEnvironmentAsync(string functionName, IReadOnlyDictionary<string, string> variables, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonLambdaClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.UpdateFunctionConfigurationAsync(
                    new UpdateFunctionConfigurationRequest
                    {
                        FunctionName = functionName,
                        Environment = new Amazon.Lambda.Model.Environment
                        {
                            Variables = new Dictionary<string, string>(variables),
                        },
                    },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<LambdaInvocationResult>> InvokeAsync(string functionName, string payload, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonLambdaClient, LambdaInvocationResult>(
            ServiceKey,
            async (client, token) =>
            {
                var request = new InvokeRequest
                {
                    FunctionName = functionName,
                    InvocationType = InvocationType.RequestResponse,
                    LogType = LogType.Tail,
                    Payload = string.IsNullOrWhiteSpace(payload) ? "{}" : payload,
                };

                var stopwatch = Stopwatch.StartNew();
                var response = await client.InvokeAsync(request, token);
                stopwatch.Stop();

                var responsePayload = response.Payload is null
                    ? string.Empty
                    : await new StreamReader(response.Payload).ReadToEndAsync(token);
                var logTail = string.IsNullOrEmpty(response.LogResult)
                    ? string.Empty
                    : Encoding.UTF8.GetString(Convert.FromBase64String(response.LogResult));

                return new LambdaInvocationResult(
                    response.StatusCode ?? 0,
                    responsePayload,
                    logTail,
                    response.FunctionError ?? string.Empty,
                    stopwatch.ElapsedMilliseconds);
            },
            cancellationToken);

    public async Task<Result> CreateFunctionAsync(LambdaFunctionCreateSpec spec, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonLambdaClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.CreateFunctionAsync(
                    new CreateFunctionRequest
                    {
                        FunctionName = spec.FunctionName,
                        Runtime = spec.Runtime,
                        Handler = spec.Handler,
                        Role = spec.Role,
                        Description = spec.Description,
                        MemorySize = spec.MemorySize,
                        Timeout = spec.Timeout,
                        Code = new FunctionCode { ZipFile = new MemoryStream(Convert.FromBase64String(spec.ZipFileBase64)) },
                    },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> UpdateConfigurationAsync(LambdaConfigurationUpdateSpec spec, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonLambdaClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.UpdateFunctionConfigurationAsync(
                    new UpdateFunctionConfigurationRequest
                    {
                        FunctionName = spec.FunctionName,
                        Runtime = spec.Runtime,
                        Handler = spec.Handler,
                        Role = spec.Role,
                        Description = spec.Description,
                        MemorySize = spec.MemorySize,
                        Timeout = spec.Timeout,
                    },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> UpdateCodeAsync(string functionName, string zipFileBase64, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonLambdaClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.UpdateFunctionCodeAsync(
                    new UpdateFunctionCodeRequest
                    {
                        FunctionName = functionName,
                        ZipFile = new MemoryStream(Convert.FromBase64String(zipFileBase64)),
                    },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteFunctionAsync(string functionName, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonLambdaClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteFunctionAsync(
                    new DeleteFunctionRequest { FunctionName = functionName }, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<IReadOnlyList<LambdaEventSourceMapping>>> ListEventSourceMappingsAsync(string functionName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonLambdaClient, IReadOnlyList<LambdaEventSourceMapping>>(
            ServiceKey,
            async (client, token) =>
            {
                var mappings = new List<LambdaEventSourceMapping>();
                string? marker = null;
                do
                {
                    var response = await client.ListEventSourceMappingsAsync(
                        new ListEventSourceMappingsRequest { FunctionName = functionName, Marker = marker }, token);
                    foreach (var configuration in response.EventSourceMappings ?? [])
                    {
                        mappings.Add(LambdaFunctionMapper.ToEventSourceMapping(configuration));
                    }

                    marker = response.NextMarker;
                }
                while (!string.IsNullOrEmpty(marker));

                return mappings;
            },
            cancellationToken);

    public Task<Result<IReadOnlyList<LambdaS3Trigger>>> ListS3TriggersAsync(string functionName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonLambdaClient, IReadOnlyList<LambdaS3Trigger>>(
            ServiceKey,
            async (client, token) =>
            {
                string? policy;
                try
                {
                    var response = await client.GetPolicyAsync(
                        new GetPolicyRequest { FunctionName = functionName }, token);
                    policy = response.Policy;
                }
                catch (Amazon.Lambda.Model.ResourceNotFoundException)
                {
                    return [];
                }

                return LambdaPolicyMapper.ParseS3Triggers(policy);
            },
            cancellationToken);

    public async Task<Result> SetEventSourceMappingStateAsync(string uuid, bool enabled, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonLambdaClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.UpdateEventSourceMappingAsync(
                    new UpdateEventSourceMappingRequest { UUID = uuid, Enabled = enabled }, token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<IReadOnlyList<LambdaLogEvent>>> GetRecentLogEventsAsync(string functionName, int limit, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonCloudWatchLogsClient, IReadOnlyList<LambdaLogEvent>>(
            LogsServiceKey,
            async (client, token) =>
            {
                var logGroupName = $"/aws/lambda/{functionName}";
                try
                {
                    var response = await client.FilterLogEventsAsync(
                        new FilterLogEventsRequest { LogGroupName = logGroupName, Limit = limit }, token);
                    var events = new List<LambdaLogEvent>();
                    foreach (var logEvent in response.Events ?? [])
                    {
                        events.Add(LambdaFunctionMapper.ToLogEvent(logEvent));
                    }

                    return events;
                }
                catch (Amazon.CloudWatchLogs.Model.ResourceNotFoundException)
                {
                    return [];
                }
            },
            cancellationToken);

    public Task<Result<IReadOnlyList<LambdaLayer>>> ListLayersAsync(string functionName, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonLambdaClient, IReadOnlyList<LambdaLayer>>(
            ServiceKey,
            async (client, token) =>
            {
                var response = await client.GetFunctionConfigurationAsync(
                    new GetFunctionConfigurationRequest { FunctionName = functionName }, token);
                var layers = new List<LambdaLayer>();
                foreach (var layer in response.Layers ?? [])
                {
                    layers.Add(LambdaFunctionMapper.ToLayer(layer));
                }

                return layers;
            },
            cancellationToken);
}
