using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.RedriveSqsMessages;
using Foundation.Application.Sqs;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.RedriveSqsMessages;

public class RedriveSqsMessagesCommandHandlerTests
{
    private readonly ISqsClient _client = Substitute.For<ISqsClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    private RedriveSqsMessagesCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, NullLogger<RedriveSqsMessagesCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenRedriveSucceeds_PublishesSuccess()
    {
        // Arrange
        _client
            .StartMessageRedriveAsync("orders-dlq", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new RedriveSqsMessagesCommand("orders-dlq"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
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
    public async Task Handle_WhenRedriveFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .StartMessageRedriveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("redrive boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new RedriveSqsMessagesCommand("orders-dlq"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("redrive boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
    }
}
