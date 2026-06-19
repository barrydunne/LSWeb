using System.Net;
using System.Net.Http.Json;
using Foundation.Api.Models;

namespace Foundation.IntegrationTests.Controllers.HappyPath;

/// <summary>
/// Happy-path round-trip for CloudWatch Logs: create a log group, confirm it appears in the listing,
/// then delete it.
/// </summary>
[Collection(IntegrationTestsCollectionDefinition.Name)]
public class CloudWatchLogsHappyPathTests
{
    private readonly IntegrationTestsFixture _fixture;

    public CloudWatchLogsHappyPathTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task CreateListDelete_RoundTripsSuccessfully()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _fixture.CreateClient();
        await _fixture.ResetCircuitBreakerAsync(cancellationToken);
        var logGroupName = $"/itest/cloudwatch/{Guid.NewGuid():N}";

        var createResponse = await client.PostAsJsonAsync(
            "/api/services/cloudwatch-logs/groups",
            new LogGroupCreateRequest(logGroupName),
            cancellationToken);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var list = await client.GetFromJsonAsync<LogGroupListResponse>(
            "/api/services/cloudwatch-logs/groups", cancellationToken);
        list.Should().NotBeNull();
        list!.LogGroups.Should().ContainSingle(group => group.Name == logGroupName);

        var deleteResponse = await client.DeleteAsync(
            $"/api/services/cloudwatch-logs/groups?logGroupName={Uri.EscapeDataString(logGroupName)}",
            cancellationToken);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
