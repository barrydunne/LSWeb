using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.CreateDynamoDbTable;
using Foundation.Application.DynamoDb;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.DynamoDb;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateDynamoDbTable;

public class CreateDynamoDbTableCommandHandlerTests
{
    private readonly IDynamoDbClient _client = Substitute.For<IDynamoDbClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private static CreateDynamoDbTableCommand BuildCommand()
        => new("orders", "pk", "S", null, null, "PAY_PER_REQUEST", null, null);

    private CreateDynamoDbTableCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh, NullLogger<CreateDynamoDbTableCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenCreateSucceeds_PublishesSuccessAndRefreshesSearch()
    {
        // Arrange
        _client
            .CreateTableAsync(Arg.Any<DynamoDbTableSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).CreateTableAsync(
            Arg.Is<DynamoDbTableSpecification>(spec => spec.TableName == "orders"),
            Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.InProgress),
            Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Succeeded),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Succeeded));
        _searchRefresh.Received(1).RequestRefresh();
    }

    [Fact]
    public async Task Handle_WhenCreateFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .CreateTableAsync(Arg.Any<DynamoDbTableSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("create boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("create boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
        _searchRefresh.DidNotReceive().RequestRefresh();
    }

    [Fact]
    public async Task Handle_MapsAllCommandFieldsOntoSpecification()
    {
        // Arrange
        _client
            .CreateTableAsync(Arg.Any<DynamoDbTableSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var command = new CreateDynamoDbTableCommand(
            "orders", "pk", "S", "sk", "N", "PROVISIONED", 5, 7);
        var sut = CreateSut();

        // Act
        await sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await _client.Received(1).CreateTableAsync(
            Arg.Is<DynamoDbTableSpecification>(spec =>
                spec.TableName == "orders"
                && spec.PartitionKeyName == "pk"
                && spec.PartitionKeyType == "S"
                && spec.SortKeyName == "sk"
                && spec.SortKeyType == "N"
                && spec.BillingMode == "PROVISIONED"
                && spec.ReadCapacityUnits == 5
                && spec.WriteCapacityUnits == 7),
            Arg.Any<CancellationToken>());
    }
}
