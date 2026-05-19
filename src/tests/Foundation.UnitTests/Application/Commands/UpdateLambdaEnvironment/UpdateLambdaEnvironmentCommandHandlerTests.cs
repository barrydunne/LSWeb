using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.UpdateLambdaEnvironment;
using Foundation.Application.Lambda;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Configuration;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UpdateLambdaEnvironment;

public class UpdateLambdaEnvironmentCommandHandlerTests
{
    private readonly ILambdaClient _client = Substitute.For<ILambdaClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    private static Result<T> Ok<T>(T value) => value;

    private UpdateLambdaEnvironmentCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, NullLogger<UpdateLambdaEnvironmentCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenUpdateSucceeds_MergesValuesAndPublishesSuccess()
    {
        // Arrange
        IReadOnlyDictionary<string, string> current = new Dictionary<string, string>
        {
            ["API_KEY"] = "old-secret",
            ["REGION"] = "eu-west-1",
        };
        _client
            .GetEnvironmentAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(current)));
        _client
            .UpdateEnvironmentAsync("orders", Arg.Any<IReadOnlyDictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var variables = new Dictionary<string, string>
        {
            ["REGION"] = "us-east-1",
            ["API_KEY"] = ConfigValue.Mask,
            ["NEW_SECRET"] = ConfigValue.Mask,
        };
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new UpdateLambdaEnvironmentCommand("orders", variables),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).UpdateEnvironmentAsync(
            "orders",
            Arg.Is<IReadOnlyDictionary<string, string>>(merged =>
                merged["REGION"] == "us-east-1"
                && merged["API_KEY"] == "old-secret"
                && merged["NEW_SECRET"] == ConfigValue.Mask),
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
    public async Task Handle_WhenGetEnvironmentFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .GetEnvironmentAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyDictionary<string, string>>>(new Error("read boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new UpdateLambdaEnvironmentCommand("orders", new Dictionary<string, string> { ["A"] = "1" }),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("read boom");
        await _client.DidNotReceive().UpdateEnvironmentAsync(
            Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, string>>(), Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
    }

    [Fact]
    public async Task Handle_WhenUpdateFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        IReadOnlyDictionary<string, string> current = new Dictionary<string, string>();
        _client
            .GetEnvironmentAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(current)));
        _client
            .UpdateEnvironmentAsync("orders", Arg.Any<IReadOnlyDictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("write boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new UpdateLambdaEnvironmentCommand("orders", new Dictionary<string, string> { ["A"] = "1" }),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("write boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
    }
}
