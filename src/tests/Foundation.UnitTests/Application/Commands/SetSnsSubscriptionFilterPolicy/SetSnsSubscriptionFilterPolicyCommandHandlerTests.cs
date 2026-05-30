using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.SetSnsSubscriptionFilterPolicy;
using Foundation.Application.Sns;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.SetSnsSubscriptionFilterPolicy;

public class SetSnsSubscriptionFilterPolicyCommandHandlerTests
{
    private readonly ISnsClient _client = Substitute.For<ISnsClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    private SetSnsSubscriptionFilterPolicyCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, NullLogger<SetSnsSubscriptionFilterPolicyCommandHandler>.Instance);

    private static SetSnsSubscriptionFilterPolicyCommand Command()
        => new("arn:aws:sns:eu-west-1:000000000000:orders-topic:8c1f", "{\"store\":[\"example_corp\"]}");

    [Fact]
    public async Task Handle_WhenSetSucceeds_PublishesSuccessAndForwardsArguments()
    {
        // Arrange
        _client
            .SetSubscriptionFilterPolicyAsync(
                "arn:aws:sns:eu-west-1:000000000000:orders-topic:8c1f",
                "{\"store\":[\"example_corp\"]}",
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(Command(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).SetSubscriptionFilterPolicyAsync(
            "arn:aws:sns:eu-west-1:000000000000:orders-topic:8c1f",
            "{\"store\":[\"example_corp\"]}",
            Arg.Any<CancellationToken>());
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
    public async Task Handle_WhenSetFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .SetSubscriptionFilterPolicyAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("set boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(Command(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("set boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
    }
}
