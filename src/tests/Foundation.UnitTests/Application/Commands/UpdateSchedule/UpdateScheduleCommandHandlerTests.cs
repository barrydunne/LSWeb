using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.UpdateSchedule;
using Foundation.Application.Scheduler;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Scheduler;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UpdateSchedule;

public class UpdateScheduleCommandHandlerTests
{
    private readonly ISchedulerClient _client = Substitute.For<ISchedulerClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private static UpdateScheduleCommand BuildCommand()
        => new(
            "daily-job",
            "default",
            "rate(10 minutes)",
            "UTC",
            "nightly run",
            null,
            null,
            "arn:aws:sqs:us-east-1:000000000000:queue",
            "arn:aws:iam::000000000000:role/scheduler",
            "FLEXIBLE",
            15,
            "DISABLED",
            null);

    private UpdateScheduleCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh, NullLogger<UpdateScheduleCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenUpdateSucceeds_PublishesSuccessAndRefreshesSearch()
    {
        // Arrange
        _client
            .UpdateScheduleAsync(Arg.Any<ScheduleSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.InProgress),
            Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Succeeded),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Succeeded));
        _searchRefresh.Received(1).RequestRefresh();
    }

    [Fact]
    public async Task Handle_WhenUpdateFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .UpdateScheduleAsync(Arg.Any<ScheduleSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("update boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("update boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
        _searchRefresh.DidNotReceive().RequestRefresh();
    }

    [Fact]
    public async Task Handle_MapsAllCommandFieldsOntoSpecification()
    {
        // Arrange
        _client
            .UpdateScheduleAsync(Arg.Any<ScheduleSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        await _client.Received(1).UpdateScheduleAsync(
            Arg.Is<ScheduleSpecification>(spec =>
                spec.Name == "daily-job"
                && spec.GroupName == "default"
                && spec.ScheduleExpression == "rate(10 minutes)"
                && spec.FlexibleTimeWindowMode == "FLEXIBLE"
                && spec.MaximumWindowInMinutes == 15
                && spec.State == "DISABLED"),
            Arg.Any<CancellationToken>());
    }
}
