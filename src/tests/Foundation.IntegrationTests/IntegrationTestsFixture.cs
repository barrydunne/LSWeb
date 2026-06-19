using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Foundation.IntegrationTests;

/// <summary>
/// Hosts the API in-process for integration tests against a dedicated LocalStack
/// container that is started before the suite and removed afterwards (whether the
/// run succeeds or fails). When a <c>LOCALSTACK_AUTH_TOKEN</c> is present on the host
/// the Pro image is used and the token is forwarded to the container; otherwise the
/// community image is used. Tests that require Pro-only services can query
/// <see cref="IsLocalStackPro"/> (or call <see cref="SkipIfLocalStackNotPro"/>) so they
/// are skipped rather than failed when running against the community edition.
/// </summary>
public sealed class IntegrationTestsFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const int EdgePort = 4566;
    private const string CommunityImage = "localstack/localstack:4.14";
    private const string ProImage = "localstack/localstack-pro:4.14";

    private static readonly HttpClient _probeClient = new() { Timeout = TimeSpan.FromSeconds(10) };

    private readonly string? _authToken = Environment.GetEnvironmentVariable("LOCALSTACK_AUTH_TOKEN");

    private IContainer? _localStack;

    /// <summary>
    /// Gets a value indicating whether the running LocalStack instance reports the Pro edition.
    /// </summary>
    public bool IsLocalStackPro { get; private set; }

    /// <summary>
    /// Starts the LocalStack container, points the in-process API at it via
    /// <c>AWS_ENDPOINT_URL</c>, and records whether the instance is Pro.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        var usePro = !string.IsNullOrWhiteSpace(_authToken);

        var builder = new ContainerBuilder()
            .WithImage(usePro ? ProImage : CommunityImage)
            .WithPortBinding(EdgePort, true)
            .WithBindMount("/var/run/docker.sock", "/var/run/docker.sock")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilHttpRequestIsSucceeded(request => request
                    .ForPort(EdgePort)
                    .ForPath("/_localstack/health")
                    .ForStatusCode(HttpStatusCode.OK)));

        if (usePro)
        {
            builder = builder.WithEnvironment("LOCALSTACK_AUTH_TOKEN", _authToken!);
        }

        _localStack = builder.Build();
        await _localStack.StartAsync();

        var endpoint = new UriBuilder("http", _localStack.Hostname, _localStack.GetMappedPublicPort(EdgePort)).Uri;
        Environment.SetEnvironmentVariable("AWS_ENDPOINT_URL", endpoint.ToString().TrimEnd('/'));

        IsLocalStackPro = await DetectProEditionAsync(endpoint);
    }

    /// <summary>
    /// Skips the calling test when the running LocalStack instance is not the Pro edition.
    /// </summary>
    public void SkipIfLocalStackNotPro()
    {
        if (!IsLocalStackPro)
        {
            Assert.Skip("Requires LocalStack Pro; set LOCALSTACK_AUTH_TOKEN to enable.");
        }
    }

    /// <summary>
    /// Resets the AWS gateway circuit breaker via the operational endpoint so that a test which
    /// expects connectivity starts from a known-closed breaker, independent of any failures that
    /// earlier tests (such as the negative reachability checks) may have recorded on the shared host.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task ResetCircuitBreakerAsync(CancellationToken cancellationToken)
    {
        using var client = CreateClient();
        using var response = await client.PostAsync("/api/system/circuit/reset", content: null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();

        if (_localStack is not null)
        {
            await _localStack.DisposeAsync();
        }
    }

    private static async Task<bool> DetectProEditionAsync(Uri endpoint)
    {
        try
        {
            var infoUri = new Uri(endpoint, "/_localstack/info");
            var info = await _probeClient.GetFromJsonAsync<LocalStackInfo>(infoUri);
            return info?.Edition is { Length: > 0 } edition
                && !edition.Contains("community", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or NotSupportedException)
        {
            return false;
        }
    }

    private sealed record LocalStackInfo(
        [property: JsonPropertyName("edition")] string? Edition);
}
