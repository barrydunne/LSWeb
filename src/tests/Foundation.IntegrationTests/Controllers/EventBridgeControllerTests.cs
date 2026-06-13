using System.Net;
using System.Net.Http.Json;
using Foundation.Api.Models;

namespace Foundation.IntegrationTests.Controllers;

[Collection(IntegrationTestsCollectionDefinition.Name)]
public class EventBridgeControllerTests
{
    private readonly IntegrationTestsFixture _fixture;

    public EventBridgeControllerTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task ListRules_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/eventbridge/rules", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<RuleListResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.Rules.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task ListTargets_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/eventbridge/targets?rule=integration-missing",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<TargetListResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.Targets.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task ListScheduledRules_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/eventbridge/scheduled-rules", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<ScheduledRuleListResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.Rules.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetScheduledRule_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/eventbridge/scheduled-rules/integration-missing",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<ScheduledRuleDetailResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task PutEvent_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new PutEventRequest(
            "integration.test", "IntegrationEvent", "{\"sample\":true}", null);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/services/eventbridge/events", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<PutEventResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task ScheduledRule_FullLifecycle_CreateTargetStateRemoveDelete()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var ruleName = $"integration-rule-{Guid.NewGuid():N}";

        // Act - create
        var createResponse = await client.PostAsJsonAsync(
            "/api/services/eventbridge/scheduled-rules",
            new ScheduledRulePutRequest(ruleName, "rate(5 minutes)", "ENABLED", "integration test rule", null),
            TestContext.Current.CancellationToken);

        // Assert - create reaches endpoint
        createResponse.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        createResponse.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (createResponse.StatusCode != HttpStatusCode.Created)
            return;

        // Act - update
        var updateResponse = await client.PutAsJsonAsync(
            $"/api/services/eventbridge/scheduled-rules/{ruleName}",
            new ScheduledRuleUpdateRequest("rate(10 minutes)", "ENABLED", "updated description"),
            TestContext.Current.CancellationToken);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act - put targets
        var queueArn = "arn:aws:sqs:eu-west-1:000000000000:integration-target-queue";
        var putTargetsResponse = await client.PutAsJsonAsync(
            $"/api/services/eventbridge/scheduled-rules/{ruleName}/targets",
            new ScheduledRuleTargetsPutRequest(
                [new ScheduledRuleTargetRequest("target-1", queueArn, null, null)]),
            TestContext.Current.CancellationToken);
        putTargetsResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act - verify target present
        var listTargets = await client.GetFromJsonAsync<TargetListResponse>(
            $"/api/services/eventbridge/targets?rule={ruleName}",
            TestContext.Current.CancellationToken);
        listTargets!.Targets.Should().Contain(_ => _.Id == "target-1");

        // Act - disable
        var stateResponse = await client.PostAsJsonAsync(
            $"/api/services/eventbridge/scheduled-rules/{ruleName}/state",
            new ScheduledRuleStateRequest("DISABLED"),
            TestContext.Current.CancellationToken);
        stateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act - remove targets
        var removeRequest = new HttpRequestMessage(
            HttpMethod.Delete, $"/api/services/eventbridge/scheduled-rules/{ruleName}/targets")
        {
            Content = JsonContent.Create(new ScheduledRuleTargetsRemoveRequest(["target-1"])),
        };
        var removeResponse = await client.SendAsync(removeRequest, TestContext.Current.CancellationToken);
        removeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act - delete rule
        var deleteResponse = await client.DeleteAsync(
            $"/api/services/eventbridge/scheduled-rules/{ruleName}",
            TestContext.Current.CancellationToken);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert - rule no longer present
        var listRules = await client.GetFromJsonAsync<ScheduledRuleListResponse>(
            "/api/services/eventbridge/scheduled-rules", TestContext.Current.CancellationToken);
        listRules!.Rules.Should().NotContain(_ => _.Name == ruleName);
    }

    [Fact]
    public async Task GetRule_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/eventbridge/rules/integration-missing",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task CreateRule_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new RulePutRequest(
            "integration-pattern-rule",
            "{\"source\":[\"integration.app\"]}",
            "ENABLED",
            null,
            null);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/services/eventbridge/rules", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task PutRuleTargets_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new RuleTargetsPutRequest(
        [
            new RuleTargetRequest(
                "t1", "arn:aws:lambda:eu-west-1:000000000000:function:fn", null, null),
        ]);

        // Act
        var response = await client.PutAsJsonAsync(
            "/api/services/eventbridge/rules/integration-missing/targets",
            request,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task RemoveRuleTargets_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new RuleTargetsRemoveRequest(["t1"]);

        // Act
        var response = await client.SendAsync(
            new HttpRequestMessage(
                HttpMethod.Delete,
                "/api/services/eventbridge/rules/integration-missing/targets")
            {
                Content = JsonContent.Create(request),
            },
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task ListEventBuses_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/eventbridge/event-buses", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<EventBusListResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.Buses.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task CreateAndDeleteEventBus_WhenRequested_ReachesEndpointsAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var busName = $"integration-bus-{Guid.NewGuid():N}";

        // Act
        var createResponse = await client.PostAsJsonAsync(
            "/api/services/eventbridge/event-buses",
            new EventBusCreateRequest(busName),
            TestContext.Current.CancellationToken);

        var deleteResponse = await client.DeleteAsync(
            $"/api/services/eventbridge/event-buses/{busName}",
            TestContext.Current.CancellationToken);

        // Assert
        createResponse.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        createResponse.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
        deleteResponse.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        deleteResponse.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }
}
