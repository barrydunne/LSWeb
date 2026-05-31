using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Foundation.Application.Configuration;
using Microsoft.Extensions.Logging;

namespace Foundation.Infrastructure.Health;

/// <summary>
/// Isolated probe against the LocalStack health endpoint (<c>/_localstack/health</c>).
/// Translates LocalStack service names into catalogue keys and reports any failure as an
/// unsuccessful probe rather than throwing, keeping the call fully isolated (NFR-7.4).
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
internal sealed partial class BackendHealthProbe : IBackendHealthProbe
{
    private static readonly Dictionary<string, string> _serviceKeyMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["logs"] = "cloudwatch-logs",
        ["dynamodb"] = "dynamodb",
        ["iam"] = "iam",
        ["lambda"] = "lambda",
        ["s3"] = "s3",
        ["secretsmanager"] = "secrets-manager",
        ["sns"] = "sns",
        ["sqs"] = "sqs",
        ["ssm"] = "ssm-parameter-store",
        ["stepfunctions"] = "step-functions",
    };

    private static readonly HashSet<string> _availableStates = new(StringComparer.OrdinalIgnoreCase) { "running", "available" };

    private static readonly IReadOnlySet<string> _emptyKeys = new HashSet<string>();

    private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(5) };

    private readonly IConfigProvider _configProvider;
    private readonly ILogger _logger;

    public BackendHealthProbe(IConfigProvider configProvider, ILogger<BackendHealthProbe> logger)
    {
        _configProvider = configProvider;
        _logger = logger;
    }

    public async Task<BackendHealthResult> ProbeAsync(CancellationToken cancellationToken)
    {
        try
        {
            var serviceUrl = _configProvider.GetSnapshot().ServiceUrl.Value;
            var healthUri = new Uri(new Uri(serviceUrl), "/_localstack/health");
            using var response = await _httpClient.GetAsync(healthUri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                LogProbeUnavailable((int)response.StatusCode);
                return new BackendHealthResult(false, _emptyKeys);
            }

            var payload = await response.Content.ReadFromJsonAsync<LocalStackHealthPayload>(cancellationToken);
            return new BackendHealthResult(true, MapAvailableKeys(payload));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogProbeFailed(exception);
            return new BackendHealthResult(false, _emptyKeys);
        }
    }

    private static IReadOnlySet<string> MapAvailableKeys(LocalStackHealthPayload? payload)
    {
        if (payload?.Services is null)
        {
            return _emptyKeys;
        }

        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, state) in payload.Services)
        {
            if (_availableStates.Contains(state) && _serviceKeyMap.TryGetValue(name, out var consoleKey))
            {
                keys.Add(consoleKey);
            }
        }

        return keys;
    }

    [LoggerMessage(LogLevel.Warning, "LocalStack health probe returned status {StatusCode}.")]
    private partial void LogProbeUnavailable(int statusCode);

    [LoggerMessage(LogLevel.Warning, "LocalStack health probe failed.")]
    private partial void LogProbeFailed(Exception exception);

    private sealed record LocalStackHealthPayload(
        [property: JsonPropertyName("services")] IReadOnlyDictionary<string, string>? Services);
}
