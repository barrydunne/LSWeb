using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.PublishSnsMessage;
using Foundation.Application.Sns;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.PublishSnsMessage;

public class PublishSnsMessageCommandHandlerTests
{
    private readonly ISnsClient _client = Substitute.For<ISnsClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    private PublishSnsMessageCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, NullLogger<PublishSnsMessageCommandHandler>.Instance);

    private static PublishSnsMessageCommand Command()
        => new(
            "arn:aws:sns:eu-west-1:000000000000:orders",
            "Subject",
            "hello",
            new Dictionary<string, string> { ["source"] = "test" });

    [Fact]
    public async Task Handle_WhenPublishSucceeds_PublishesSuccessAndForwardsArguments()
    {
        // Arrange
        _client
            .PublishAsync(
                "arn:aws:sns:eu-west-1:000000000000:orders",
                "Subject",
                "hello",
                Arg.Any<IReadOnlyDictionary<string, string>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(Command(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).PublishAsync(
            "arn:aws:sns:eu-west-1:000000000000:orders",
            "Subject",
            "hello",
            Arg.Is<IReadOnlyDictionary<string, string>>(attributes => attributes["source"] == "test"),
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
    public async Task Handle_WhenPublishFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .PublishAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string>(),
                Arg.Any<IReadOnlyDictionary<string, string>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("publish boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(Command(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("publish boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
    }
}
