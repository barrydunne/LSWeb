using System.Net;
using System.Net.Http.Json;
using Foundation.Api.Models;

namespace Foundation.IntegrationTests.Controllers;

[Collection(IntegrationTestsCollectionDefinition.Name)]
public class SnsControllerTests
{
    private readonly IntegrationTestsFixture _fixture;

    public SnsControllerTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task ListTopics_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/sns/topics", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<SnsTopicListResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.Topics.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task CreateTopic_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new SnsTopicCreateRequest("integration-create-topic");

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/services/sns/topics", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task DeleteTopic_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.DeleteAsync(
            "/api/services/sns/topics?arn=arn:aws:sns:eu-west-1:000000000000:integration-missing-topic",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task ListSubscriptions_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/sns/subscriptions?arn=arn:aws:sns:eu-west-1:000000000000:integration-subscriptions-topic",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<SnsSubscriptionListResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.Subscriptions.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task PublishMessage_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new SnsPublishMessageRequest(
            "arn:aws:sns:eu-west-1:000000000000:integration-publish-topic",
            "Integration",
            "hello",
            new Dictionary<string, string> { ["source"] = "integration" });

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/services/sns/topics/messages", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task GetFilterPolicy_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/sns/subscriptions/filter-policy?arn=arn:aws:sns:eu-west-1:000000000000:integration-topic:8c1f",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<SnsSubscriptionFilterPolicyResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.FilterPolicy.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task SetFilterPolicy_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var request = new SnsSubscriptionFilterPolicyRequest(
            "arn:aws:sns:eu-west-1:000000000000:integration-topic:8c1f",
            "{\"source\":[\"integration\"]}");

        // Act
        var response = await client.PutAsJsonAsync(
            "/api/services/sns/subscriptions/filter-policy", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }
}
