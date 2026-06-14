using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.UpdateStateMachineDefinition;
using Foundation.Application.StepFunctions;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UpdateStateMachineDefinition;

public class UpdateStateMachineDefinitionCommandHandlerTests
{
    private readonly IStepFunctionsClient _client = Substitute.For<IStepFunctionsClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    private const string Arn = "arn:aws:states:eu-west-1:000000000000:stateMachine:orders";
    private const string Definition = "{\"StartAt\":\"A\",\"States\":{\"A\":{\"Type\":\"Pass\",\"End\":true}}}";

    private static UpdateStateMachineDefinitionCommand BuildCommand()
        => new(Arn, Definition);

    private UpdateStateMachineDefinitionCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, NullLogger<UpdateStateMachineDefinitionCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenUpdateSucceeds_PublishesSuccess()
    {
        // Arrange
        _client
            .UpdateStateMachineDefinitionAsync(Arn, Definition, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).UpdateStateMachineDefinitionAsync(Arn, Definition, Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Succeeded),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Succeeded));
    }

    [Fact]
    public async Task Handle_WhenUpdateFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .UpdateStateMachineDefinitionAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("update boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("update boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
    }
}
