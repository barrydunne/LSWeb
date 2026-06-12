using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Cognito;
using Foundation.Application.Commands.SetCognitoUserPassword;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.SetCognitoUserPassword;

public class SetCognitoUserPasswordCommandHandlerTests
{
    private readonly ICognitoClient _client = Substitute.For<ICognitoClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    private SetCognitoUserPasswordCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, NullLogger<SetCognitoUserPasswordCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenSucceeds_PublishesSuccessAndForwardsArguments()
    {
        // Arrange
        _client
            .SetUserPasswordAsync("eu-west-1_abc123", "alice", "NewPass1!", true, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new SetCognitoUserPasswordCommand("eu-west-1_abc123", "alice", "NewPass1!", true),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).SetUserPasswordAsync(
            "eu-west-1_abc123", "alice", "NewPass1!", true, Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Succeeded),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Succeeded));
    }

    [Fact]
    public async Task Handle_WhenFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .SetUserPasswordAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("password boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new SetCognitoUserPasswordCommand("eu-west-1_abc123", "alice", "NewPass1!", false),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("password boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
    }
}
