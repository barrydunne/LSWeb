using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.UnsubscribeSnsTopic;
using Foundation.Application.Sns;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UnsubscribeSnsTopic;

public class UnsubscribeSnsTopicCommandHandlerTests
{
    private readonly ISnsClient _client = Substitute.For<ISnsClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    private const string SubscriptionArn = "arn:aws:sns:eu-west-1:000000000000:topic:sub-id";

    private UnsubscribeSnsTopicCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, NullLogger<UnsubscribeSnsTopicCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenUnsubscribeSucceeds_PublishesSuccess()
    {
        // Arrange
        _client
            .UnsubscribeAsync(SubscriptionArn, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new UnsubscribeSnsTopicCommand(SubscriptionArn), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).UnsubscribeAsync(SubscriptionArn, Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Succeeded),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Succeeded));
    }

    [Fact]
    public async Task Handle_WhenUnsubscribeFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .UnsubscribeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("unsubscribe boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new UnsubscribeSnsTopicCommand(SubscriptionArn), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("unsubscribe boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
    }
}
