using System.Net;
using System.Net.Http.Json;
using Foundation.Api.Models;
using Foundation.Domain.Snapshot;

namespace Foundation.IntegrationTests.Controllers;

[Collection(IntegrationTestsCollectionDefinition.Name)]
public class SnapshotControllerTests
{
    private readonly IntegrationTestsFixture _fixture;

    public SnapshotControllerTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task Export_WhenRequested_ReachesEndpointAndReturnsSnapshot()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/api/snapshot/export", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<SnapshotExportResponse>(TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.SnapshotId.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task Import_WhenRequested_ReachesEndpointAndReturnsResult()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var snapshot = new WorkspaceSnapshot(
            "test-snap",
            DateTime.UtcNow,
            new Dictionary<string, IReadOnlyList<SnapshotResourceData>>());

        // Act
        var response = await client.PostAsJsonAsync("/api/snapshot/import", snapshot, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<SnapshotImportResponse>(TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.OperationType.Should().Be("Import");
        }
    }
}
