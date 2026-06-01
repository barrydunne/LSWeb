using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.CloudFormation;
using Foundation.Application.Commands.DetectStackDrift;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DetectStackDrift;

public class DetectStackDriftCommandHandlerTests
{
    private readonly ICloudFormationClient _client = Substitute.For<ICloudFormationClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    private DetectStackDriftCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog,
            NullLogger<DetectStackDriftCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenDetectSucceeds_PublishesSuccessAndReturnsDetectionId()
    {
        // Arrange
        _client
            .DetectStackDriftAsync("orders-stack", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("drift-123"));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new DetectStackDriftCommand("orders-stack"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("drift-123");
        await _client.Received(1).DetectStackDriftAsync(
            "orders-stack", Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.InProgress),
            Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Succeeded),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Succeeded));
    }

    [Fact]
    public async Task Handle_WhenDetectFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .DetectStackDriftAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>(new Error("detect boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new DetectStackDriftCommand("orders-stack"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("detect boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
    }
}
