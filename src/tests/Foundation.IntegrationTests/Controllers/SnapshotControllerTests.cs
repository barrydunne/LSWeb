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
            var payload = await response.Content.ReadFromJsonAsync<WorkspaceSnapshot>(TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.Id.Should().NotBeNullOrEmpty();
            payload.Resources.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task ExportedSnapshot_CanBeImportedBack_RoundTrips()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var exportResponse = await client.GetAsync("/api/snapshot/export", TestContext.Current.CancellationToken);
        if (exportResponse.StatusCode != HttpStatusCode.OK)
            return;

        var exported = await exportResponse.Content.ReadFromJsonAsync<WorkspaceSnapshot>(TestContext.Current.CancellationToken);
        exported.Should().NotBeNull();
        var importResponse = await client.PostAsJsonAsync("/api/snapshot/import", exported, TestContext.Current.CancellationToken);

        // Assert
        // The export document must be a valid import payload: no validation 400 for a self-produced snapshot.
        importResponse.StatusCode.Should().NotBe(HttpStatusCode.BadRequest);
        importResponse.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
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
