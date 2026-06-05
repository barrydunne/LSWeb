using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Scheduler;
using Foundation.Domain.Scheduler;
using Foundation.Infrastructure.Search;

namespace Foundation.UnitTests.Infrastructure.Search;

public class SchedulerResourceSourceTests
{
    private readonly ISchedulerClient _client = Substitute.For<ISchedulerClient>();

    private SchedulerResourceSource CreateSut()
        => new(_client);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public void ServiceKey_IsScheduler()
        => CreateSut().ServiceKey.Should().Be("scheduler");

    [Fact]
    public async Task ListAsync_WhenClientSucceeds_MapsSchedulesToSearchEntries()
    {
        // Arrange
        IReadOnlyList<ScheduleSummary> schedules =
        [
            new(
                "nightly run",
                "default",
                "ENABLED",
                "arn:aws:lambda:eu-west-1:000000000000:function:run",
                "arn:aws:scheduler:eu-west-1:000000000000:schedule/default/nightly run"),
        ];
        _client
            .ListSchedulesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(schedules)));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        var entry = entries.Should().ContainSingle().Subject;
        entry.ServiceKey.Should().Be("scheduler");
        entry.ResourceId.Should().Be("nightly run");
        entry.DisplayName.Should().Be("nightly run");
        entry.Route.Should().Be("/services/scheduler/default%2Fnightly%20run");
    }

    [Fact]
    public async Task ListAsync_WhenClientFails_ReturnsEmptyList()
    {
        // Arrange
        _client
            .ListSchedulesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<ScheduleSummary>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        entries.Should().BeEmpty();
    }
}
