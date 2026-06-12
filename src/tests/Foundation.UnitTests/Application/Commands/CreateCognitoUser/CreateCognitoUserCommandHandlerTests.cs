using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Cognito;
using Foundation.Application.Commands.CreateCognitoUser;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Cognito;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateCognitoUser;

public class CreateCognitoUserCommandHandlerTests
{
    private readonly ICognitoClient _client = Substitute.For<ICognitoClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    private static CreateCognitoUserCommand BuildCommand()
        => new("eu-west-1_abc123", "alice", [new CognitoUserAttributeEntry("email", "alice@example.com")], "Temp123!");

    private static CognitoUserDetail Detail()
        => new(
            "alice",
            "FORCE_CHANGE_PASSWORD",
            true,
            [new CognitoUserAttributeEntry("email", "alice@example.com")],
            DateTimeOffset.UnixEpoch,
            DateTimeOffset.UnixEpoch);

    private CreateCognitoUserCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, NullLogger<CreateCognitoUserCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenCreateSucceeds_PublishesSuccess()
    {
        // Arrange
        _client
            .CreateUserAsync(Arg.Any<CognitoUserSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<CognitoUserDetail>>(Detail()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Username.Should().Be("alice");
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
    public async Task Handle_WhenCreateFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .CreateUserAsync(Arg.Any<CognitoUserSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<CognitoUserDetail>>(new Error("create boom")));
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
    }

    [Fact]
    public async Task Handle_MapsAllCommandFieldsOntoSpecification()
    {
        // Arrange
        _client
            .CreateUserAsync(Arg.Any<CognitoUserSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<CognitoUserDetail>>(Detail()));
        var sut = CreateSut();

        // Act
        await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        await _client.Received(1).CreateUserAsync(
            Arg.Is<CognitoUserSpecification>(specification =>
                specification.UserPoolId == "eu-west-1_abc123"
                && specification.Username == "alice"
                && specification.Attributes.Count == 1
                && specification.Attributes[0].Name == "email"
                && specification.Attributes[0].Value == "alice@example.com"
                && specification.TemporaryPassword == "Temp123!"),
            Arg.Any<CancellationToken>());
    }
}
