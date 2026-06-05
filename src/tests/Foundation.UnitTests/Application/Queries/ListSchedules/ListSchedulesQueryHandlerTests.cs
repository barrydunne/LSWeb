using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.ListSchedules;
using Foundation.Application.Scheduler;
using Foundation.Domain.Scheduler;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListSchedules;

public class ListSchedulesQueryHandlerTests
{
    private readonly ISchedulerClient _client = Substitute.For<ISchedulerClient>();

    private ListSchedulesQueryHandler CreateSut()
        => new(_client, NullLogger<ListSchedulesQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsSchedules()
    {
        // Arrange
        IReadOnlyList<ScheduleSummary> schedules =
        [
            new(
                "nightly",
                "default",
                "ENABLED",
                "arn:aws:lambda:eu-west-1:000000000000:function:run",
                "arn:aws:scheduler:eu-west-1:000000000000:schedule/default/nightly"),
        ];
        _client
            .ListSchedulesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(schedules)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListSchedulesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Schedules.Should().ContainSingle(_ => _.Name == "nightly");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListSchedulesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<ScheduleSummary>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListSchedulesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
