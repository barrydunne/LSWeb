using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.ExecuteDynamoDbTransaction;
using Foundation.Application.DynamoDb;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.DynamoDb;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.ExecuteDynamoDbTransaction;

public class ExecuteDynamoDbTransactionCommandHandlerTests
{
    private readonly IDynamoDbClient _client = Substitute.For<IDynamoDbClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    private static ExecuteDynamoDbTransactionCommand BuildCommand()
        => new([new DynamoDbTransactionAction("Put", "orders", "{\"pk\":{\"S\":\"a\"}}")]);

    private ExecuteDynamoDbTransactionCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, NullLogger<ExecuteDynamoDbTransactionCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenTransactionSucceeds_PublishesSuccessAndLogsActivity()
    {
        // Arrange
        _client
            .ExecuteTransactionWriteAsync(Arg.Any<IReadOnlyList<DynamoDbTransactionAction>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).ExecuteTransactionWriteAsync(
            Arg.Is<IReadOnlyList<DynamoDbTransactionAction>>(actions => actions.Count == 1),
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
    public async Task Handle_WhenTransactionFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .ExecuteTransactionWriteAsync(Arg.Any<IReadOnlyList<DynamoDbTransactionAction>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("cancelled")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("cancelled");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
    }
}
