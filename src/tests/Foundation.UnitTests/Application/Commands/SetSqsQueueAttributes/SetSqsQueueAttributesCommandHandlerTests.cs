using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.SetSqsQueueAttributes;
using Foundation.Application.Sqs;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.SetSqsQueueAttributes;

public class SetSqsQueueAttributesCommandHandlerTests
{
    private readonly ISqsClient _client = Substitute.For<ISqsClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    private SetSqsQueueAttributesCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, NullLogger<SetSqsQueueAttributesCommandHandler>.Instance);

    private static SetSqsQueueAttributesCommand Command()
        => new("orders", 45, 86400, 10, 5);

    [Fact]
    public async Task Handle_WhenUpdateSucceeds_PublishesSuccessAndForwardsAttributes()
    {
        // Arrange
        _client
            .SetQueueAttributesAsync(
                "orders", Arg.Any<IReadOnlyDictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(Command(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).SetQueueAttributesAsync(
            "orders",
            Arg.Is<IReadOnlyDictionary<string, string>>(attributes =>
                attributes["VisibilityTimeout"] == "45"
                && attributes["MessageRetentionPeriod"] == "86400"
                && attributes["DelaySeconds"] == "10"
                && attributes["ReceiveMessageWaitTimeSeconds"] == "5"),
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
    public async Task Handle_WhenUpdateFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .SetQueueAttributesAsync(
                Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, string>>(), Arg.Any<CancellationToken>())
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
