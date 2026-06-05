using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.ListScheduleGroups;
using Foundation.Application.Scheduler;
using Foundation.Domain.Scheduler;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListScheduleGroups;

public class ListScheduleGroupsQueryHandlerTests
{
    private readonly ISchedulerClient _client = Substitute.For<ISchedulerClient>();

    private ListScheduleGroupsQueryHandler CreateSut()
        => new(_client, NullLogger<ListScheduleGroupsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsGroups()
    {
        // Arrange
        IReadOnlyList<ScheduleGroup> groups =
        [
            new(
                "default",
                "ACTIVE",
                "arn:aws:scheduler:eu-west-1:000000000000:schedule-group/default",
                DateTimeOffset.UnixEpoch,
                DateTimeOffset.UnixEpoch),
        ];
        _client
            .ListScheduleGroupsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(groups)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListScheduleGroupsQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Groups.Should().ContainSingle(_ => _.Name == "default");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListScheduleGroupsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<ScheduleGroup>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListScheduleGroupsQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
