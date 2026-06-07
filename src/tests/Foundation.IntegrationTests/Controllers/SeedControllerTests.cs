using System.Net;
using System.Net.Http.Json;
using Foundation.Api.Models;

namespace Foundation.IntegrationTests.Controllers;

[Collection(IntegrationTestsCollectionDefinition.Name)]
public class SeedControllerTests
{
    private readonly IntegrationTestsFixture _fixture;

    public SeedControllerTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task Templates_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/api/seed/templates", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<SeedTemplatesResponse>(TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.Templates.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task Apply_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.PostAsync(
            "/api/seed/templates/messaging-starter/apply",
            content: null,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<SeedOutcomeResponse>(TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.TemplateId.Should().Be("messaging-starter");
            payload.Items.Should().NotBeNull();
        }
    }
}
