using System.Net;
using System.Net.Http.Json;

namespace Foundation.IntegrationTests.Controllers;

[Collection(IntegrationTestsCollectionDefinition.Name)]
public class SystemControllerTests
{
    private const string HeaderName = "X-Correlation-ID";

    private readonly IntegrationTestsFixture _fixture;

    public SystemControllerTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task GetLiveness_WhenServiceIsRunning_ReturnsHealthyWithCorrelationHeader()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/api/system/liveness", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey(HeaderName);

        var payload = await response.Content.ReadFromJsonAsync<LivenessResponse>(TestContext.Current.CancellationToken);
        payload.Should().NotBeNull();
        payload!.Status.Should().Be("Healthy");
    }

    [Fact]
    public async Task GetHealth_WhenServiceIsRunning_ReturnsServiceAvailabilitySnapshot()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/api/system/health", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<HealthSnapshotResponse>(TestContext.Current.CancellationToken);
        payload.Should().NotBeNull();
        payload!.Services.Should().NotBeEmpty();
        payload.Services.Should().OnlyContain(_ =>
            _.Availability == "Available" || _.Availability == "Unavailable" || _.Availability == "Unknown");
    }

    [Fact]
    public async Task GetConnectivity_WhenServiceIsRunning_ReturnsResolvedTargetWithoutCredentials()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/api/system/connectivity", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<ConnectivityResponse>(TestContext.Current.CancellationToken);
        payload.Should().NotBeNull();
        payload!.Status.Should().BeOneOf("Connected", "Disconnected");
        payload.Endpoint.Should().NotBeNullOrWhiteSpace();
        payload.Region.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetDiagnostics_WhenServiceIsRunning_ReturnsSnapshotWithSensitiveValuesMasked()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/api/system/diagnostics", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<DiagnosticsSnapshotResponse>(TestContext.Current.CancellationToken);
        payload.Should().NotBeNull();
        payload!.Configuration.Should().NotBeEmpty();
        payload.Endpoint.Should().NotBeNullOrWhiteSpace();
        payload.Region.Should().NotBeNullOrWhiteSpace();
        payload.ConnectivityStatus.Should().BeOneOf("Connected", "Disconnected");
        payload.Configuration.Should().Contain(_ => _.IsSensitive);
        payload.Configuration.Where(_ => _.IsSensitive).Should().OnlyContain(_ => _.Value == "********");
    }

    [Fact]
    public async Task RefreshCatalogue_WhenServiceIsRunning_ReturnsAccepted()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.PostAsync("/api/system/catalogue/refresh", content: null, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }

    [Fact]
    public async Task GetActivity_AfterRefresh_ReturnsRecordedEntry()
    {
        // Arrange
        var client = _fixture.CreateClient();
        await client.PostAsync("/api/system/catalogue/refresh", content: null, TestContext.Current.CancellationToken);

        // Act
        var response = await client.GetAsync("/api/system/activity", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<ActivityLogResponse>(TestContext.Current.CancellationToken);
        payload.Should().NotBeNull();
        payload!.Entries.Should().Contain(_ => _.Operation == "catalogue-refresh" && _.State == "Succeeded");
    }

    [Fact]
    public async Task GenerateCliSnippet_WhenServiceIsRunning_ReturnsRunnableCommandWithoutSecrets()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new
        {
            service = "s3api",
            operation = "head-bucket",
            parameters = new[]
            {
                new { name = "bucket", value = "my-bucket", isSensitive = false },
                new { name = "token-code", value = "supersecret", isSensitive = true },
            },
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/system/cli-snippet", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<CliSnippetResponse>(TestContext.Current.CancellationToken);
        payload.Should().NotBeNull();
        payload!.Command.Should().StartWith("aws s3api head-bucket");
        payload.Command.Should().Contain("--bucket my-bucket");
        payload.Command.Should().Contain("--endpoint-url");
        payload.Command.Should().Contain("--region");
        payload.Command.Should().Contain("--token-code <token-code>");
        payload.Command.Should().NotContain("supersecret");
    }

    private sealed record LivenessResponse(string Status);

    private sealed record HealthSnapshotResponse(IReadOnlyList<ServiceAvailabilityResponse> Services);

    private sealed record ServiceAvailabilityResponse(string Key, string Availability);

    private sealed record ConnectivityResponse(string Status, string Endpoint, string Region, string? Error);

    private sealed record DiagnosticsSnapshotResponse(
        IReadOnlyList<DiagnosticsConfigResponse> Configuration,
        string Endpoint,
        string Region,
        string ConnectivityStatus,
        string? ConnectivityError,
        bool RevealAllowed);

    private sealed record DiagnosticsConfigResponse(string Name, string Value, string Source, bool IsSensitive);

    private sealed record CliSnippetResponse(string Command);

    private sealed record ActivityLogResponse(IReadOnlyList<ActivityEntryResponse> Entries);

    private sealed record ActivityEntryResponse(string OperationId, string Operation, string State, string Message, DateTimeOffset OccurredAt);
}
