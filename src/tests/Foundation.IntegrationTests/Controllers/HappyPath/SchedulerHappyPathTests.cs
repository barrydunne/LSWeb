using System.Net;
using System.Net.Http.Json;
using Foundation.Api.Models;

namespace Foundation.IntegrationTests.Controllers.HappyPath;

/// <summary>
/// Happy-path round-trip for EventBridge Scheduler: create a schedule, confirm it is listed and
/// retrievable, then delete it.
/// </summary>
[Collection(IntegrationTestsCollectionDefinition.Name)]
public class SchedulerHappyPathTests
{
    private readonly IntegrationTestsFixture _fixture;

    public SchedulerHappyPathTests(IntegrationTestsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task CreateListGetDelete_RoundTripsSuccessfully()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _fixture.CreateClient();
        await _fixture.ResetCircuitBreakerAsync(cancellationToken);
        var scheduleName = $"itest-scheduler-{Guid.NewGuid():N}";
        const string groupName = "default";
        const string targetArn = "arn:aws:lambda:eu-west-1:000000000000:function:itest-target";
        const string roleArn = "arn:aws:iam::000000000000:role/itest-scheduler-role";

        var createResponse = await client.PostAsJsonAsync(
            "/api/services/scheduler/schedules",
            new ScheduleCreateRequest(
                scheduleName,
                groupName,
                "rate(1 hour)",
                null,
                "Integration test schedule",
                null,
                null,
                targetArn,
                roleArn,
                "OFF",
                null,
                "ENABLED",
                null),
            cancellationToken);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var list = await client.GetFromJsonAsync<ScheduleListResponse>(
            "/api/services/scheduler/schedules", cancellationToken);
        list.Should().NotBeNull();
        list!.Schedules.Should().ContainSingle(schedule => schedule.Name == scheduleName);

        var detail = await client.GetFromJsonAsync<ScheduleDetailResponse>(
            $"/api/services/scheduler/schedule?name={Uri.EscapeDataString(scheduleName)}&group={Uri.EscapeDataString(groupName)}",
            cancellationToken);
        detail.Should().NotBeNull();
        detail!.Name.Should().Be(scheduleName);

        var deleteResponse = await client.DeleteAsync(
            $"/api/services/scheduler/schedules/{Uri.EscapeDataString(scheduleName)}?group={Uri.EscapeDataString(groupName)}",
            cancellationToken);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
