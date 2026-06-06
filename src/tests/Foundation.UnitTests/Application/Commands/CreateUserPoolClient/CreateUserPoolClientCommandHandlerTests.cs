using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Cognito;
using Foundation.Application.Commands.CreateUserPoolClient;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Cognito;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateUserPoolClient;

public class CreateUserPoolClientCommandHandlerTests
{
    private readonly ICognitoClient _client = Substitute.For<ICognitoClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private static CreateUserPoolClientCommand BuildCommand()
        => new("eu-west-1_abc123", "web", true, ["ALLOW_USER_SRP_AUTH"], ["code"], ["openid"], ["https://app/callback"], true);

    private static UserPoolClientDetail Detail()
        => new(
            "client-1",
            "web",
            "eu-west-1_abc123",
            "secret",
            true,
            ["ALLOW_USER_SRP_AUTH"],
            ["code"],
            ["openid"],
            ["https://app/callback"],
            true,
            DateTimeOffset.UnixEpoch,
            DateTimeOffset.UnixEpoch);

    private CreateUserPoolClientCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh, NullLogger<CreateUserPoolClientCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenCreateSucceeds_PublishesSuccessAndRefreshesSearch()
    {
        // Arrange
        _client
            .CreateUserPoolClientAsync(Arg.Any<UserPoolClientSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<UserPoolClientDetail>>(Detail()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ClientId.Should().Be("client-1");
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
            .CreateUserPoolClientAsync(Arg.Any<UserPoolClientSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<UserPoolClientDetail>>(new Error("create boom")));
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
            .CreateUserPoolClientAsync(Arg.Any<UserPoolClientSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<UserPoolClientDetail>>(Detail()));
        var sut = CreateSut();

        // Act
        await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        await _client.Received(1).CreateUserPoolClientAsync(
            Arg.Is<UserPoolClientSpecification>(specification =>
                specification.UserPoolId == "eu-west-1_abc123"
                && specification.ClientName == "web"
                && specification.GenerateSecret
                && specification.ExplicitAuthFlows.Count == 1
                && specification.AllowedOAuthFlows[0] == "code"
                && specification.AllowedOAuthScopes[0] == "openid"
                && specification.CallbackURLs[0] == "https://app/callback"
                && specification.AllowedOAuthFlowsUserPoolClient
                && specification.ClientId == null),
            Arg.Any<CancellationToken>());
    }
}
