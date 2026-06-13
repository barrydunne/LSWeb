using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.CreateDynamoDbIndex;
using Foundation.Application.DynamoDb;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.DynamoDb;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateDynamoDbIndex;

public class CreateDynamoDbIndexCommandHandlerTests
{
    private readonly IDynamoDbClient _client = Substitute.For<IDynamoDbClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    private static CreateDynamoDbIndexCommand BuildCommand()
        => new("orders", "gsi-1", "gpk", "S", null, null, "ALL");

    private CreateDynamoDbIndexCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, NullLogger<CreateDynamoDbIndexCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenCreateSucceeds_PublishesSuccessAndLogsActivity()
    {
        // Arrange
        _client
            .CreateGlobalSecondaryIndexAsync(Arg.Any<DynamoDbIndexSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).CreateGlobalSecondaryIndexAsync(
            Arg.Is<DynamoDbIndexSpecification>(spec => spec.IndexName == "gsi-1" && spec.TableName == "orders"),
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
    public async Task Handle_MapsAllCommandFieldsOntoSpecification()
    {
        // Arrange
        _client
            .CreateGlobalSecondaryIndexAsync(Arg.Any<DynamoDbIndexSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var command = new CreateDynamoDbIndexCommand("orders", "gsi-1", "gpk", "S", "gsk", "N", "KEYS_ONLY");
        var sut = CreateSut();

        // Act
        await sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await _client.Received(1).CreateGlobalSecondaryIndexAsync(
            Arg.Is<DynamoDbIndexSpecification>(spec =>
                spec.TableName == "orders"
                && spec.IndexName == "gsi-1"
                && spec.PartitionKeyName == "gpk"
                && spec.PartitionKeyType == "S"
                && spec.SortKeyName == "gsk"
                && spec.SortKeyType == "N"
                && spec.ProjectionType == "KEYS_ONLY"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCreateFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .CreateGlobalSecondaryIndexAsync(Arg.Any<DynamoDbIndexSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("index boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("index boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
    }
}
