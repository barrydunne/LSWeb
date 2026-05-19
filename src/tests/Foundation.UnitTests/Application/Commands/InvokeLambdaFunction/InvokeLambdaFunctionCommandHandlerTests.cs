using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.InvokeLambdaFunction;
using Foundation.Application.Lambda;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Lambda;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.InvokeLambdaFunction;

public class InvokeLambdaFunctionCommandHandlerTests
{
    private readonly ILambdaClient _client = Substitute.For<ILambdaClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    private static Result<T> Ok<T>(T value) => value;

    private InvokeLambdaFunctionCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, NullLogger<InvokeLambdaFunctionCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenInvocationSucceeds_PublishesSuccessAndReturnsResult()
    {
        // Arrange
        var invocation = new LambdaInvocationResult(200, "{\"ok\":true}", "REPORT Duration: 12 ms", string.Empty, 15);
        _client
            .InvokeAsync("orders", "{}", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(invocation)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new InvokeLambdaFunctionCommand("orders", "{}"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.StatusCode.Should().Be(200);
        result.Value.Payload.Should().Be("{\"ok\":true}");
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
    public async Task Handle_WhenFunctionReportsError_PublishesFailureAndReturnsResult()
    {
        // Arrange
        var invocation = new LambdaInvocationResult(200, "{\"errorMessage\":\"boom\"}", "log", "Unhandled", 8);
        _client
            .InvokeAsync("orders", "{}", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(invocation)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new InvokeLambdaFunctionCommand("orders", "{}"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FunctionError.Should().Be("Unhandled");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
    }

    [Fact]
    public async Task Handle_WhenInvocationFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .InvokeAsync("orders", "{}", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<LambdaInvocationResult>>(new Error("invoke boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new InvokeLambdaFunctionCommand("orders", "{}"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("invoke boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.InProgress),
            Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
    }
}
