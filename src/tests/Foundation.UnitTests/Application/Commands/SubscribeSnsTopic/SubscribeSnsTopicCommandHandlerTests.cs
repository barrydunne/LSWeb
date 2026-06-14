using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.SubscribeSnsTopic;
using Foundation.Application.Sns;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.SubscribeSnsTopic;

public class SubscribeSnsTopicCommandHandlerTests
{
    private readonly ISnsClient _client = Substitute.For<ISnsClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    private static SubscribeSnsTopicCommand BuildCommand()
        => new("arn:aws:sns:eu-west-1:000000000000:topic", "sqs", "arn:aws:sqs:eu-west-1:000000000000:q");

    private SubscribeSnsTopicCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, NullLogger<SubscribeSnsTopicCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenSubscribeSucceeds_PublishesSuccess()
    {
        // Arrange
        _client
            .SubscribeAsync(
                "arn:aws:sns:eu-west-1:000000000000:topic",
                "sqs",
                "arn:aws:sqs:eu-west-1:000000000000:q",
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Succeeded),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Succeeded));
    }

    [Fact]
    public async Task Handle_WhenSubscribeFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .SubscribeAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("subscribe boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("subscribe boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
    }
}
