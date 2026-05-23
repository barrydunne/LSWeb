using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.CreateSqsQueue;
using Foundation.Application.Search;
using Foundation.Application.Sqs;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateSqsQueue;

public class CreateSqsQueueCommandHandlerTests
{
    private readonly ISqsClient _client = Substitute.For<ISqsClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private CreateSqsQueueCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh, NullLogger<CreateSqsQueueCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenCreateSucceeds_PublishesSuccessAndRefreshesSearch()
    {
        // Arrange
        _client
            .CreateQueueAsync("orders", false, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new CreateSqsQueueCommand("orders", false),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).CreateQueueAsync("orders", false, Arg.Any<CancellationToken>());
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
    public async Task Handle_WhenFifoRequested_PassesFifoFlag()
    {
        // Arrange
        _client
            .CreateQueueAsync("orders.fifo", true, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new CreateSqsQueueCommand("orders.fifo", true),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).CreateQueueAsync("orders.fifo", true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCreateFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .CreateQueueAsync("orders", false, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("create boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new CreateSqsQueueCommand("orders", false),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("create boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
        _searchRefresh.DidNotReceive().RequestRefresh();
    }
}
