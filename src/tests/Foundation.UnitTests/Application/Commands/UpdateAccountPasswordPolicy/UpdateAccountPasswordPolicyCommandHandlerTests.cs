using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Commands.UpdateAccountPasswordPolicy;
using Foundation.Application.Iam;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Iam;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UpdateAccountPasswordPolicy;

public class UpdateAccountPasswordPolicyCommandHandlerTests
{
    private readonly IIamClient _client = Substitute.For<IIamClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();

    private static UpdateAccountPasswordPolicyCommand BuildCommand(int? maxPasswordAge = 90)
        => new(
            MinimumPasswordLength: 14,
            RequireSymbols: true,
            RequireNumbers: true,
            RequireUppercaseCharacters: true,
            RequireLowercaseCharacters: true,
            AllowUsersToChangePassword: true,
            MaxPasswordAge: maxPasswordAge,
            PasswordReusePrevention: 5,
            HardExpiry: false);

    private UpdateAccountPasswordPolicyCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, NullLogger<UpdateAccountPasswordPolicyCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenUpdateSucceeds_PublishesSuccessAndDerivesExpiry()
    {
        // Arrange
        _client
            .UpdateAccountPasswordPolicyAsync(Arg.Any<IamPasswordPolicy>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).UpdateAccountPasswordPolicyAsync(
            Arg.Is<IamPasswordPolicy>(policy => policy.ExpirePasswords && policy.MaxPasswordAge == 90),
            Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Succeeded),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Succeeded));
    }

    [Fact]
    public async Task Handle_WhenNoMaxPasswordAge_DoesNotExpirePasswords()
    {
        // Arrange
        _client
            .UpdateAccountPasswordPolicyAsync(Arg.Any<IamPasswordPolicy>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(maxPasswordAge: null), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).UpdateAccountPasswordPolicyAsync(
            Arg.Is<IamPasswordPolicy>(policy => !policy.ExpirePasswords && policy.MaxPasswordAge == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUpdateFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .UpdateAccountPasswordPolicyAsync(Arg.Any<IamPasswordPolicy>(), Arg.Any<CancellationToken>())
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
