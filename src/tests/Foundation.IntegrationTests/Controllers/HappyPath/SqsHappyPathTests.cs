using System.Net;
using System.Net.Http.Json;
using Foundation.Api.Models;

namespace Foundation.IntegrationTests.Controllers.HappyPath;

/// <summary>
/// Happy-path round-trip for SQS: create a queue, confirm it appears in the listing and exposes
/// its attributes, send a message, then delete it. Asserts on the real backend results rather than
/// only confirming the endpoints are routed.
/// </summary>
[Collection(IntegrationTestsCollectionDefinition.Name)]
public class SqsHappyPathTests
{
    private readonly IntegrationTestsFixture _fixture;

    public SqsHappyPathTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task CreateListGetSendDelete_RoundTripsSuccessfully()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _fixture.CreateClient();
        await _fixture.ResetCircuitBreakerAsync(cancellationToken);
        var queueName = $"itest-sqs-{Guid.NewGuid():N}";

        // Create
        var createResponse = await client.PostAsJsonAsync(
            "/api/services/sqs/queues",
            new SqsQueueCreateRequest(queueName, false),
            cancellationToken);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // List - the new queue is present
        var list = await client.GetFromJsonAsync<SqsQueueListResponse>(
            "/api/services/sqs/queues", cancellationToken);
        list.Should().NotBeNull();
        list!.Queues.Should().ContainSingle(queue => queue.Name == queueName);

        // Get attributes - reflects the created queue
        var attributesResponse = await client.GetAsync(
            $"/api/services/sqs/queues/{queueName}/attributes", cancellationToken);
        attributesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var attributes = await attributesResponse.Content.ReadFromJsonAsync<SqsQueueAttributesResponse>(
            cancellationToken);
        attributes.Should().NotBeNull();
        attributes!.QueueArn.Should().EndWith(queueName);
        attributes.FifoQueue.Should().BeFalse();

        // Send a message
        var sendResponse = await client.PostAsJsonAsync(
            $"/api/services/sqs/queues/{queueName}/messages",
            new SqsSendMessageRequest("integration-happy-path", null, null, null),
            cancellationToken);
        sendResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

        // Delete - cleans up and confirms deletion succeeds
        var deleteResponse = await client.DeleteAsync(
            $"/api/services/sqs/queues/{queueName}", cancellationToken);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
