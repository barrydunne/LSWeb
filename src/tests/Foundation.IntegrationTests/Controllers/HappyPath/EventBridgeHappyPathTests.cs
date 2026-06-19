using System.Net;
using System.Net.Http.Json;
using Foundation.Api.Models;

namespace Foundation.IntegrationTests.Controllers.HappyPath;

/// <summary>
/// Happy-path round-trip for EventBridge: create an event bus, confirm it appears in the listing,
/// then delete it.
/// </summary>
[Collection(IntegrationTestsCollectionDefinition.Name)]
public class EventBridgeHappyPathTests
{
    private readonly IntegrationTestsFixture _fixture;

    public EventBridgeHappyPathTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task CreateListDelete_RoundTripsSuccessfully()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _fixture.CreateClient();
        await _fixture.ResetCircuitBreakerAsync(cancellationToken);
        var busName = $"itest-eventbridge-{Guid.NewGuid():N}";

        var createResponse = await client.PostAsJsonAsync(
            "/api/services/eventbridge/event-buses",
            new EventBusCreateRequest(busName),
            cancellationToken);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var list = await client.GetFromJsonAsync<EventBusListResponse>(
            "/api/services/eventbridge/event-buses",
            cancellationToken);
        list.Should().NotBeNull();
        list!.Buses.Should().ContainSingle(bus => bus.Name == busName);

        var deleteResponse = await client.DeleteAsync(
            $"/api/services/eventbridge/event-buses/{Uri.EscapeDataString(busName)}",
            cancellationToken);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
