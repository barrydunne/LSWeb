using System.Net;
using System.Net.Http.Json;
using Foundation.Api.Models;

namespace Foundation.IntegrationTests.Controllers.HappyPath;

/// <summary>
/// Happy-path round-trip for SNS: create a topic, confirm it appears in the listing, then delete it.
/// </summary>
[Collection(IntegrationTestsCollectionDefinition.Name)]
public class SnsHappyPathTests
{
    private readonly IntegrationTestsFixture _fixture;

    public SnsHappyPathTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task CreateListDelete_RoundTripsSuccessfully()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _fixture.CreateClient();
        await _fixture.ResetCircuitBreakerAsync(cancellationToken);
        var topicName = $"itest-sns-{Guid.NewGuid():N}";

        var createResponse = await client.PostAsJsonAsync(
            "/api/services/sns/topics",
            new SnsTopicCreateRequest(topicName),
            cancellationToken);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var list = await client.GetFromJsonAsync<SnsTopicListResponse>(
            "/api/services/sns/topics",
            cancellationToken);
        list.Should().NotBeNull();
        var createdTopic = list!.Topics.Should().ContainSingle(topic => topic.Name == topicName).Subject;

        var deleteResponse = await client.DeleteAsync(
            $"/api/services/sns/topics?arn={Uri.EscapeDataString(createdTopic.TopicArn)}",
            cancellationToken);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
