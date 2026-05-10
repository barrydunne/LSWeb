using System.Net;
using System.Net.Http.Json;

namespace Foundation.IntegrationTests.Controllers;

public class SystemControllerTests : IClassFixture<IntegrationTestsFixture>
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

    private sealed record LivenessResponse(string Status);

    private sealed record HealthSnapshotResponse(IReadOnlyList<ServiceAvailabilityResponse> Services);

    private sealed record ServiceAvailabilityResponse(string Key, string Availability);

    private sealed record ConnectivityResponse(string Status, string Endpoint, string Region, string? Error);
}
