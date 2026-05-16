using System.Net;
using System.Net.Http.Json;

namespace Foundation.IntegrationTests.Controllers;

[Collection(IntegrationTestsCollectionDefinition.Name)]
public class UserControllerTests
{
    private readonly IntegrationTestsFixture _fixture;

    public UserControllerTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task RecordRecentlyViewed_ThenGet_ReturnsTheReference()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var reference = $"sqs://recent-{Guid.NewGuid():N}";

        // Act
        var recordResponse = await client.PostAsJsonAsync(
            "/api/user/recently-viewed",
            new ReferenceRequest(reference),
            TestContext.Current.CancellationToken);

        // Assert
        recordResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var payload = await client.GetFromJsonAsync<ReferenceListResponse>(
            "/api/user/recently-viewed", TestContext.Current.CancellationToken);
        payload.Should().NotBeNull();
        payload!.References.Should().Contain(reference);
    }

    [Fact]
    public async Task AddFavourite_ThenGet_ReturnsTheReference()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var reference = $"s3://fav-{Guid.NewGuid():N}";

        // Act
        var addResponse = await client.PutAsJsonAsync(
            "/api/user/favourites",
            new ReferenceRequest(reference),
            TestContext.Current.CancellationToken);

        // Assert
        addResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var payload = await client.GetFromJsonAsync<ReferenceListResponse>(
            "/api/user/favourites", TestContext.Current.CancellationToken);
        payload.Should().NotBeNull();
        payload!.References.Should().Contain(reference);
    }

    [Fact]
    public async Task RemoveFavourite_AfterAdding_RemovesTheReference()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var reference = $"s3://remove-{Guid.NewGuid():N}";
        await client.PutAsJsonAsync(
            "/api/user/favourites",
            new ReferenceRequest(reference),
            TestContext.Current.CancellationToken);

        // Act
        var removeResponse = await client.SendAsync(
            new HttpRequestMessage(HttpMethod.Delete, "/api/user/favourites")
            {
                Content = JsonContent.Create(new ReferenceRequest(reference)),
            },
            TestContext.Current.CancellationToken);

        // Assert
        removeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var payload = await client.GetFromJsonAsync<ReferenceListResponse>(
            "/api/user/favourites", TestContext.Current.CancellationToken);
        payload.Should().NotBeNull();
        payload!.References.Should().NotContain(reference);
    }

    private sealed record ReferenceRequest(string Reference);

    private sealed record ReferenceListResponse(IReadOnlyList<string> References);
}
