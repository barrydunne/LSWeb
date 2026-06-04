using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.PutEventBridgeEvent;
using Foundation.Application.EventBridge;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.EventBridge;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.PutEventBridgeEvent;

public class PutEventBridgeEventCommandHandlerTests
{
    private readonly IEventBridgeClient _client = Substitute.For<IEventBridgeClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    private const string Source = "orders.service";
    private const string DetailType = "OrderPlaced";
    private const string Detail = "{\"orderId\":\"abc\"}";

    private PutEventBridgeEventCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, NullLogger<PutEventBridgeEventCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenAccepted_PublishesSuccessAndReturnsResult()
    {
        // Arrange
        var putResult = new EventBridgePutResult("event-1", 0, null, null);
        _client
            .PutEventAsync(Source, DetailType, Detail, "orders-bus", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<EventBridgePutResult>>(putResult));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new PutEventBridgeEventCommand(Source, DetailType, Detail, "orders-bus"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.EventId.Should().Be("event-1");
        result.Value.Accepted.Should().BeTrue();
        await _client.Received(1)
            .PutEventAsync(Source, DetailType, Detail, "orders-bus", Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Succeeded),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Succeeded));
    }

    [Fact]
    public async Task Handle_WhenClientFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .PutEventAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<EventBridgePutResult>>(new Error("put boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new PutEventBridgeEventCommand(Source, DetailType, Detail, null),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("put boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
    }

    [Fact]
    public async Task Handle_WhenEntryRejected_PublishesFailureAndReturnsResult()
    {
        // Arrange
        var putResult = new EventBridgePutResult(null, 1, "InternalException", "rejected");
        _client
            .PutEventAsync(Source, DetailType, Detail, null, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<EventBridgePutResult>>(putResult));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new PutEventBridgeEventCommand(Source, DetailType, Detail, null),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Accepted.Should().BeFalse();
        result.Value.ErrorMessage.Should().Be("rejected");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
    }

    [Fact]
    public async Task Handle_WhenEntryRejectedWithoutMessage_UsesErrorCode()
    {
        // Arrange
        var putResult = new EventBridgePutResult(null, 1, "InternalException", null);
        _client
            .PutEventAsync(Source, DetailType, Detail, null, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<EventBridgePutResult>>(putResult));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new PutEventBridgeEventCommand(Source, DetailType, Detail, null),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Accepted.Should().BeFalse();
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEntryRejectedWithoutCodeOrMessage_PublishesFailure()
    {
        // Arrange
        var putResult = new EventBridgePutResult(null, 1, null, null);
        _client
            .PutEventAsync(Source, DetailType, Detail, null, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<EventBridgePutResult>>(putResult));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new PutEventBridgeEventCommand(Source, DetailType, Detail, null),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Accepted.Should().BeFalse();
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
    }
}
