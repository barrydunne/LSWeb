using System.Net;
using System.Net.Http.Json;
using Foundation.Api.Models;
namespace Foundation.IntegrationTests.Controllers;

[Collection(IntegrationTestsCollectionDefinition.Name)]
public class CloudWatchLogsControllerTests
{
    private readonly IntegrationTestsFixture _fixture;

    public CloudWatchLogsControllerTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task ListGroups_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/cloudwatch-logs/groups", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<LogGroupListResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.LogGroups.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task ListStreams_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/cloudwatch-logs/streams?logGroupName=/aws/lambda/missing",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<LogStreamListResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.LogStreams.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetEvents_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/cloudwatch-logs/events?logGroupName=/aws/lambda/missing&logStreamName=missing&limit=10",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<LogEventListResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.Events.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task FilterEvents_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(
            "/api/services/cloudwatch-logs/filter?logGroupName=/aws/lambda/missing&filterPattern=ERROR&startTime=0&limit=10",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var payload = await response.Content.ReadFromJsonAsync<LogEventListResponse>(
                TestContext.Current.CancellationToken);
            payload.Should().NotBeNull();
            payload!.Events.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task CreateGroup_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/services/cloudwatch-logs/groups",
            new LogGroupCreateRequest("integration-create-log-group-probe"),
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task DeleteGroup_WhenRequested_ReachesEndpointAndReturnsDefinedStatus()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.DeleteAsync(
            "/api/services/cloudwatch-logs/groups?logGroupName=missing-log-group",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }
}
