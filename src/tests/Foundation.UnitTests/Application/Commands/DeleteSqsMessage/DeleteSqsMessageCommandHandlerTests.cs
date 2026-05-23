using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.DeleteSqsMessage;
using Foundation.Application.Sqs;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.DeleteSqsMessage;

public class DeleteSqsMessageCommandHandlerTests
{
    private readonly ISqsClient _client = Substitute.For<ISqsClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    private DeleteSqsMessageCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, NullLogger<DeleteSqsMessageCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenDeleteSucceeds_PublishesSuccess()
    {
        // Arrange
        _client
            .DeleteMessageAsync("orders", "receipt-1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new DeleteSqsMessageCommand("orders", "receipt-1"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).DeleteMessageAsync("orders", "receipt-1", Arg.Any<CancellationToken>());
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
    public async Task Handle_WhenDeleteFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .DeleteMessageAsync("orders", "receipt-1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("delete boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new DeleteSqsMessageCommand("orders", "receipt-1"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("delete boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
    }
}
