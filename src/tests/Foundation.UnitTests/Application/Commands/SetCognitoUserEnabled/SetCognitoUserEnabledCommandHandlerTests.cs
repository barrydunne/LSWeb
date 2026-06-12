using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Cognito;
using Foundation.Application.Commands.SetCognitoUserEnabled;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.SetCognitoUserEnabled;

public class SetCognitoUserEnabledCommandHandlerTests
{
    private readonly ICognitoClient _client = Substitute.For<ICognitoClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    private SetCognitoUserEnabledCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, NullLogger<SetCognitoUserEnabledCommandHandler>.Instance);

    [Theory]
    [InlineData(true, "Enabled alice.")]
    [InlineData(false, "Disabled alice.")]
    public async Task Handle_WhenSucceeds_PublishesSuccessWithOutcomeMessage(bool enabled, string expectedMessage)
    {
        // Arrange
        _client
            .SetUserEnabledAsync("eu-west-1_abc123", "alice", enabled, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new SetCognitoUserEnabledCommand("eu-west-1_abc123", "alice", enabled),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).SetUserEnabledAsync(
            "eu-west-1_abc123", "alice", enabled, Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification =>
                notification.State == OperationState.Succeeded && notification.Message == expectedMessage),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Succeeded));
    }

    [Fact]
    public async Task Handle_WhenFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .SetUserEnabledAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("enabled boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new SetCognitoUserEnabledCommand("eu-west-1_abc123", "alice", true),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("enabled boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
    }
}
