using System.Net;
using System.Net.Http.Json;

namespace Foundation.IntegrationTests.Controllers;

[Collection(IntegrationTestsCollectionDefinition.Name)]
public class NavigationControllerTests
{
    private readonly IntegrationTestsFixture _fixture;

    public NavigationControllerTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task Resolve_WithValidArn_ReturnsResolvedRoute()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/navigation/resolve?ref=arn:aws:sqs:eu-west-1:000000000000:orders",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<ResolveReferenceResponse>(TestContext.Current.CancellationToken);
        payload.Should().NotBeNull();
        payload!.ServiceKey.Should().Be("sqs");
        payload.ResourceId.Should().Be("orders");
        payload.Route.Should().Be("/services/sqs/orders");
    }

    [Fact]
    public async Task Resolve_WithBareIdAndServiceHint_ReturnsResolvedRoute()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/navigation/resolve?ref=orders&service=sqs",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<ResolveReferenceResponse>(TestContext.Current.CancellationToken);
        payload.Should().NotBeNull();
        payload!.ServiceKey.Should().Be("sqs");
        payload.ResourceId.Should().Be("orders");
        payload.Route.Should().Be("/services/sqs/orders");
    }

    [Fact]
    public async Task Resolve_WithUnsupportedReference_ReturnsError()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/navigation/resolve?ref=arn:aws:kinesis:eu-west-1:000000000000:stream/foo",
            TestContext.Current.CancellationToken);

        // Assert
        ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
    }

    private sealed record ResolveReferenceResponse(string ServiceKey, string ResourceId, string Route);
}
