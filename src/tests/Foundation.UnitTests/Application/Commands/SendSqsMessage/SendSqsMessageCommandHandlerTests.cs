using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.SendSqsMessage;
using Foundation.Application.Sqs;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.SendSqsMessage;

public class SendSqsMessageCommandHandlerTests
{
    private readonly ISqsClient _client = Substitute.For<ISqsClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    private SendSqsMessageCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, NullLogger<SendSqsMessageCommandHandler>.Instance);

    private static SendSqsMessageCommand Command()
        => new("orders", "hello", new Dictionary<string, string> { ["source"] = "test" }, null, null);

    [Fact]
    public async Task Handle_WhenSendSucceeds_PublishesSuccessAndForwardsArguments()
    {
        // Arrange
        _client
            .SendMessageAsync(
                "orders",
                "hello",
                Arg.Any<IReadOnlyDictionary<string, string>>(),
                null,
                null,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(Command(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).SendMessageAsync(
            "orders",
            "hello",
            Arg.Is<IReadOnlyDictionary<string, string>>(attributes => attributes["source"] == "test"),
            null,
            null,
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
    public async Task Handle_WhenSendFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .SendMessageAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<IReadOnlyDictionary<string, string>>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("send boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(Command(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("send boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
    }
}
