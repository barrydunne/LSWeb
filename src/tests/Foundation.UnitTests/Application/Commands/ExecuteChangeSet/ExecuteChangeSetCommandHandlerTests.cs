using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.CloudFormation;
using Foundation.Application.Commands.ExecuteChangeSet;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.ExecuteChangeSet;

public class ExecuteChangeSetCommandHandlerTests
{
    private readonly ICloudFormationClient _client = Substitute.For<ICloudFormationClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private ExecuteChangeSetCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh,
            NullLogger<ExecuteChangeSetCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenExecuteSucceeds_PublishesSuccessAndRefreshesSearch()
    {
        // Arrange
        _client
            .ExecuteChangeSetAsync("orders-stack", "add-queue", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ExecuteChangeSetCommand("orders-stack", "add-queue"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).ExecuteChangeSetAsync(
            "orders-stack", "add-queue", Arg.Any<CancellationToken>());
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
    public async Task Handle_WhenExecuteFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .ExecuteChangeSetAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("execute boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ExecuteChangeSetCommand("orders-stack", "add-queue"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("execute boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
        _searchRefresh.DidNotReceive().RequestRefresh();
    }
}
