using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Cognito;
using Foundation.Application.Commands.CreateUserPool;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Cognito;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateUserPool;

public class CreateUserPoolCommandHandlerTests
{
    private readonly ICognitoClient _client = Substitute.For<ICognitoClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private static CreateUserPoolCommand BuildCommand()
        => new("customers", "OFF", ["email"], ["email"]);

    private CreateUserPoolCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh, NullLogger<CreateUserPoolCommandHandler>.Instance);

    [Fact]
    public async Task Handle_ForwardsPasswordPolicyOntoSpecification()
    {
        // Arrange
        _client
            .CreateUserPoolAsync(Arg.Any<UserPoolSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("eu-west-1_abc123"));
        var sut = CreateSut();
        var command = new CreateUserPoolCommand(
            "customers", "OFF", ["email"], ["email"], new PasswordPolicy(10, true, false, true, false));

        // Act
        await sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await _client.Received(1).CreateUserPoolAsync(
            Arg.Is<UserPoolSpecification>(specification =>
                specification.PasswordPolicy != null
                && specification.PasswordPolicy.MinimumLength == 10
                && specification.PasswordPolicy.RequireUppercase
                && !specification.PasswordPolicy.RequireLowercase
                && specification.PasswordPolicy.RequireNumbers
                && !specification.PasswordPolicy.RequireSymbols),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCreateSucceeds_PublishesSuccessAndRefreshesSearch()
    {
        // Arrange
        _client
            .CreateUserPoolAsync(Arg.Any<UserPoolSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("eu-west-1_abc123"));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("eu-west-1_abc123");
        await _client.Received(1).CreateUserPoolAsync(
            Arg.Is<UserPoolSpecification>(specification => specification.Name == "customers"),
            Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.InProgress),
            Arg.Any<CancellationToken>());
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
            .CreateUserPoolAsync(Arg.Any<UserPoolSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>(new Error("create boom")));
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

    [Fact]
    public async Task Handle_MapsAllCommandFieldsOntoSpecification()
    {
        // Arrange
        _client
            .CreateUserPoolAsync(Arg.Any<UserPoolSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("eu-west-1_abc123"));
        var command = new CreateUserPoolCommand(
            "customers", "OPTIONAL", ["email", "phone_number"], ["email"]);
        var sut = CreateSut();

        // Act
        await sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await _client.Received(1).CreateUserPoolAsync(
            Arg.Is<UserPoolSpecification>(specification =>
                specification.Name == "customers"
                && specification.MfaConfiguration == "OPTIONAL"
                && specification.UsernameAttributes.Count == 2
                && specification.UsernameAttributes[0] == "email"
                && specification.UsernameAttributes[1] == "phone_number"
                && specification.AutoVerifiedAttributes.Count == 1
                && specification.AutoVerifiedAttributes[0] == "email"),
            Arg.Any<CancellationToken>());
    }
}
