using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.CreateEventBridgeEventBus;
using Foundation.Application.EventBridge;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateEventBridgeEventBus;

public class CreateEventBridgeEventBusCommandHandlerTests
{
    private readonly IEventBridgeClient _client = Substitute.For<IEventBridgeClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    private CreateEventBridgeEventBusCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog,
            NullLogger<CreateEventBridgeEventBusCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenCreateSucceeds_PublishesSuccessAndLogsActivity()
    {
        // Arrange
        _client
            .CreateEventBusAsync("orders-bus", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new CreateEventBridgeEventBusCommand("orders-bus"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).CreateEventBusAsync("orders-bus", Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Succeeded),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Succeeded));
    }

    [Fact]
    public async Task Handle_WhenCreateFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .CreateEventBusAsync("orders-bus", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("bus boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new CreateEventBridgeEventBusCommand("orders-bus"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("bus boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
    }
}
