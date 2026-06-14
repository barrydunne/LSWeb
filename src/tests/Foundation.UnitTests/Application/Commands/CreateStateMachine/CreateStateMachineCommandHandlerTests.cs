using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.CreateStateMachine;
using Foundation.Application.Search;
using Foundation.Application.StepFunctions;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.StepFunctions;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateStateMachine;

public class CreateStateMachineCommandHandlerTests
{
    private readonly IStepFunctionsClient _client = Substitute.For<IStepFunctionsClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private const string Definition = "{\"StartAt\":\"A\",\"States\":{\"A\":{\"Type\":\"Pass\",\"End\":true}}}";
    private const string RoleArn = "arn:aws:iam::000000000000:role/sfn";

    private static CreateStateMachineCommand BuildCommand()
        => new("orders", Definition, RoleArn, "STANDARD");

    private static Result<T> Ok<T>(T value)
        => value;

    private CreateStateMachineCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh, NullLogger<CreateStateMachineCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenCreateSucceeds_PublishesSuccessAndRefreshesSearch()
    {
        // Arrange
        var created = new StateMachineCreateResult("arn:aws:states:eu-west-1:000000000000:stateMachine:orders", DateTimeOffset.UtcNow);
        _client
            .CreateStateMachineAsync("orders", Definition, RoleArn, "STANDARD", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(created)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.StateMachineArn.Should().Be("arn:aws:states:eu-west-1:000000000000:stateMachine:orders");
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
            .CreateStateMachineAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<StateMachineCreateResult>>(new Error("create boom")));
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
}
