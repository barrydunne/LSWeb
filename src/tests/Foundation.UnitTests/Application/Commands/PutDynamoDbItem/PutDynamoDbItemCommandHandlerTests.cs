using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.PutDynamoDbItem;
using Foundation.Application.DynamoDb;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.PutDynamoDbItem;

public class PutDynamoDbItemCommandHandlerTests
{
    private readonly IDynamoDbClient _client = Substitute.For<IDynamoDbClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    private static PutDynamoDbItemCommand BuildCommand(string? conditionExpression = null)
        => new("orders", "{\"id\":\"a\"}", conditionExpression);

    private PutDynamoDbItemCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, NullLogger<PutDynamoDbItemCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenPutSucceeds_PublishesSuccessAndAppendsActivity()
    {
        // Arrange
        _client
            .PutItemAsync("orders", "{\"id\":\"a\"}", null, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).PutItemAsync("orders", "{\"id\":\"a\"}", null, Arg.Any<CancellationToken>());
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
    public async Task Handle_WhenConditionExpressionProvided_PassesItToTheClient()
    {
        // Arrange
        _client
            .PutItemAsync("orders", "{\"id\":\"a\"}", "attribute_not_exists(id)", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            BuildCommand("attribute_not_exists(id)"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).PutItemAsync(
            "orders", "{\"id\":\"a\"}", "attribute_not_exists(id)", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPutFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .PutItemAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("put boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("put boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
    }
}
