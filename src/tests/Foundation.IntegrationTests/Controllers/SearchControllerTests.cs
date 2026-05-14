using System.Net;
using System.Net.Http.Json;

namespace Foundation.IntegrationTests.Controllers;

[Collection(IntegrationTestsCollectionDefinition.Name)]
public class SearchControllerTests
{
    private readonly IntegrationTestsFixture _fixture;

    public SearchControllerTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task Search_WithBlankTerm_ReturnsNoMatches()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/api/search?q=", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<SearchResponse>(TestContext.Current.CancellationToken);
        payload.Should().NotBeNull();
        payload!.Matches.Should().BeEmpty();
    }

    [Fact]
    public async Task Search_WithMissingTerm_ReturnsNoMatches()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/api/search", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<SearchResponse>(TestContext.Current.CancellationToken);
        payload.Should().NotBeNull();
        payload!.Matches.Should().BeEmpty();
    }

    [Fact]
    public async Task Refresh_WhenServiceIsRunning_ReturnsAccepted()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.PostAsync("/api/search/refresh", content: null, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }

    [Fact]
    public async Task State_WhenServiceIsRunning_ReturnsIndexState()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/api/search/state", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<SearchStateResponse>(TestContext.Current.CancellationToken);
        payload.Should().NotBeNull();
        payload!.EntryCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task Refresh_AfterRequest_RecordsActivityEntry()
    {
        // Arrange
        var client = _fixture.CreateClient();
        await client.PostAsync("/api/search/refresh", content: null, TestContext.Current.CancellationToken);

        // Act
        var response = await client.GetAsync("/api/system/activity", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<ActivityLogResponse>(TestContext.Current.CancellationToken);
        payload.Should().NotBeNull();
        payload!.Entries.Should().Contain(_ => _.Operation == "search-refresh" && _.State == "Succeeded");
    }

    private sealed record SearchResponse(IReadOnlyList<SearchMatchResponse> Matches);

    private sealed record SearchMatchResponse(string ServiceKey, string ResourceId, string DisplayName, string Route);

    private sealed record SearchStateResponse(DateTimeOffset BuiltAt, int EntryCount, bool IsBuilding);

    private sealed record ActivityLogResponse(IReadOnlyList<ActivityEntryResponse> Entries);

    private sealed record ActivityEntryResponse(string OperationId, string Operation, string State, string Message, DateTimeOffset OccurredAt);
}
