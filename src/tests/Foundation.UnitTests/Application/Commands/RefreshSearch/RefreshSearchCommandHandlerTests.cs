using Foundation.Application.Activity;
using Foundation.Application.Commands.RefreshSearch;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.RefreshSearch;

public class RefreshSearchCommandHandlerTests
{
    private readonly ISearchRefreshTrigger _trigger = Substitute.For<ISearchRefreshTrigger>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    [Fact]
    public async Task Handle_WhenInvoked_RequestsARefresh()
    {
        // Arrange
        var sut = new RefreshSearchCommandHandler(_trigger, _publisher, _activityLog, NullLogger<RefreshSearchCommandHandler>.Instance);

        // Act
        var result = await sut.Handle(new RefreshSearchCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _trigger.Received(1).RequestRefresh();
    }

    [Fact]
    public async Task Handle_WhenInvoked_PublishesInProgressThenSucceededForOneOperation()
    {
        // Arrange
        var published = new List<Notification>();
        _publisher
            .PublishAsync(Arg.Do<Notification>(_ => published.Add(_)), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        var sut = new RefreshSearchCommandHandler(_trigger, _publisher, _activityLog, NullLogger<RefreshSearchCommandHandler>.Instance);

        // Act
        await sut.Handle(new RefreshSearchCommand(), TestContext.Current.CancellationToken);

        // Assert
        published.Should().HaveCount(2);
        published[0].State.Should().Be(OperationState.InProgress);
        published[1].State.Should().Be(OperationState.Succeeded);
        published[0].Operation.Should().Be("search-refresh");
        published[1].Operation.Should().Be("search-refresh");
        published[1].OperationId.Should().Be(published[0].OperationId);
    }

    [Fact]
    public async Task Handle_WhenInvoked_AppendsSucceededEntryToActivityLog()
    {
        // Arrange
        var appended = new List<ActivityEntry>();
        _activityLog
            .When(_ => _.Append(Arg.Any<ActivityEntry>()))
            .Do(_ => appended.Add(_.Arg<ActivityEntry>()));
        var sut = new RefreshSearchCommandHandler(_trigger, _publisher, _activityLog, NullLogger<RefreshSearchCommandHandler>.Instance);

        // Act
        await sut.Handle(new RefreshSearchCommand(), TestContext.Current.CancellationToken);

        // Assert
        var entry = appended.Should().ContainSingle().Subject;
        entry.Operation.Should().Be("search-refresh");
        entry.State.Should().Be(OperationState.Succeeded);
        entry.Message.Should().Be("Search index refresh requested.");
    }
}
