using Foundation.Application.Activity;
using Foundation.Application.Commands.ExecuteBulkAction;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.ExecuteBulkAction;

public class ExecuteBulkActionCommandHandlerTests
{
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    [Fact]
    public async Task Handle_WhenAllResourcesValid_PublishesInProgressThenSucceeded()
    {
        // Arrange
        var published = new List<Notification>();
        _publisher
            .PublishAsync(Arg.Do<Notification>(_ => published.Add(_)), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        var sut = new ExecuteBulkActionCommandHandler(_publisher, _activityLog, NullLogger<ExecuteBulkActionCommandHandler>.Instance);

        // Act
        var result = await sut.Handle(
            new ExecuteBulkActionCommand("delete", ["a", "b"]), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Action.Should().Be("delete");
        result.Value.TotalCount.Should().Be(2);
        result.Value.SucceededCount.Should().Be(2);
        result.Value.FailedCount.Should().Be(0);
        result.Value.OverallState.Should().Be(OperationState.Succeeded);

        published.Should().HaveCount(2);
        published[0].State.Should().Be(OperationState.InProgress);
        published[1].State.Should().Be(OperationState.Succeeded);
        published[0].Operation.Should().Be("bulk-action");
        published[1].OperationId.Should().Be(published[0].OperationId);
        result.Value.OperationId.Should().Be(published[0].OperationId);
    }

    [Fact]
    public async Task Handle_WhenSomeResourcesInvalid_ReturnsPartialFailureWithErrors()
    {
        // Arrange
        var published = new List<Notification>();
        _publisher
            .PublishAsync(Arg.Do<Notification>(_ => published.Add(_)), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        var sut = new ExecuteBulkActionCommandHandler(_publisher, _activityLog, NullLogger<ExecuteBulkActionCommandHandler>.Instance);

        // Act
        var result = await sut.Handle(
            new ExecuteBulkActionCommand("delete", ["a", "   ", null!]), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(3);
        result.Value.SucceededCount.Should().Be(1);
        result.Value.FailedCount.Should().Be(2);
        result.Value.OverallState.Should().Be(OperationState.Failed);

        result.Value.Items[0].Should().BeEquivalentTo(new { ResourceId = "a", Succeeded = true, Error = (string?)null });
        result.Value.Items[1].Should().BeEquivalentTo(new { ResourceId = "   ", Succeeded = false, Error = "Resource id is required." });
        result.Value.Items[2].Should().BeEquivalentTo(new { ResourceId = string.Empty, Succeeded = false, Error = "Resource id is required." });

        published[^1].State.Should().Be(OperationState.Failed);
    }

    [Fact]
    public async Task Handle_WhenInvoked_AppendsTerminalEntryToActivityLog()
    {
        // Arrange
        var appended = new List<ActivityEntry>();
        _activityLog
            .When(_ => _.Append(Arg.Any<ActivityEntry>()))
            .Do(_ => appended.Add(_.Arg<ActivityEntry>()));
        var sut = new ExecuteBulkActionCommandHandler(_publisher, _activityLog, NullLogger<ExecuteBulkActionCommandHandler>.Instance);

        // Act
        await sut.Handle(new ExecuteBulkActionCommand("delete", ["a"]), TestContext.Current.CancellationToken);

        // Assert
        var entry = appended.Should().ContainSingle().Subject;
        entry.Operation.Should().Be("bulk-action");
        entry.State.Should().Be(OperationState.Succeeded);
        entry.Message.Should().Be("'delete' completed: 1 succeeded, 0 failed.");
    }
}
