using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.GetSchedule;
using Foundation.Application.Scheduler;
using Foundation.Domain.Scheduler;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetSchedule;

public class GetScheduleQueryHandlerTests
{
    private readonly ISchedulerClient _client = Substitute.For<ISchedulerClient>();

    private GetScheduleQueryHandler CreateSut()
        => new(_client, NullLogger<GetScheduleQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsSchedule()
    {
        // Arrange
        var schedule = new ScheduleDetail(
            "nightly",
            "default",
            "ENABLED",
            "rate(1 day)",
            "UTC",
            "Nightly run",
            DateTimeOffset.UnixEpoch,
            null,
            "arn:aws:lambda:eu-west-1:000000000000:function:run",
            "arn:aws:iam::000000000000:role/scheduler",
            "OFF",
            null,
            "arn:aws:scheduler:eu-west-1:000000000000:schedule/default/nightly",
            DateTimeOffset.UnixEpoch,
            DateTimeOffset.UnixEpoch,
            null);
        _client
            .GetScheduleAsync("nightly", "default", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(schedule)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetScheduleQuery("nightly", "default"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Schedule.Name.Should().Be("nightly");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetScheduleAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ScheduleDetail>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetScheduleQuery("nightly", "default"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
