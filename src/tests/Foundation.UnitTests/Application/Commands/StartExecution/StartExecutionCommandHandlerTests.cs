using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.StartExecution;
using Foundation.Application.StepFunctions;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.StepFunctions;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.StartExecution;

public class StartExecutionCommandHandlerTests
{
    private readonly IStepFunctionsClient _client = Substitute.For<IStepFunctionsClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    private const string Arn = "arn:aws:states:eu-west-1:000000000000:stateMachine:orders-workflow";

    private StartExecutionCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, NullLogger<StartExecutionCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenStartSucceeds_PublishesSuccessAndReturnsResult()
    {
        // Arrange
        var startResult = new ExecutionStartResult(
            "arn:aws:states:eu-west-1:000000000000:execution:orders-workflow:run-1",
            DateTimeOffset.UnixEpoch);
        _client
            .StartExecutionAsync(Arn, "run-1", "{\"key\":\"value\"}", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ExecutionStartResult>>(startResult));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new StartExecutionCommand(Arn, "run-1", "{\"key\":\"value\"}"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ExecutionArn.Should()
            .Be("arn:aws:states:eu-west-1:000000000000:execution:orders-workflow:run-1");
        await _client.Received(1)
            .StartExecutionAsync(Arn, "run-1", "{\"key\":\"value\"}", Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Succeeded),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Succeeded));
    }

    [Fact]
    public async Task Handle_WhenStartFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .StartExecutionAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ExecutionStartResult>>(new Error("start boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new StartExecutionCommand(Arn, null, null),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("start boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
    }
}
