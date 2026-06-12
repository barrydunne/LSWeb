using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Cognito;
using Foundation.Application.Commands.RegenerateUserPoolClientSecret;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Cognito;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.RegenerateUserPoolClientSecret;

public class RegenerateUserPoolClientSecretCommandHandlerTests
{
    private readonly ICognitoClient _client = Substitute.For<ICognitoClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private static UserPoolClientDetail Detail()
        => new(
            "client-2",
            "web",
            "eu-west-1_abc123",
            "new-secret",
            true,
            ["ALLOW_USER_SRP_AUTH"],
            ["code"],
            ["openid"],
            ["https://app/callback"],
            true,
            DateTimeOffset.UnixEpoch,
            DateTimeOffset.UnixEpoch);

    private RegenerateUserPoolClientSecretCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh, NullLogger<RegenerateUserPoolClientSecretCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenSucceeds_PublishesSuccessRefreshesSearchAndReturnsNewClient()
    {
        // Arrange
        _client
            .RegenerateUserPoolClientSecretAsync("eu-west-1_abc123", "client-1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<UserPoolClientDetail>>(Detail()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new RegenerateUserPoolClientSecretCommand("eu-west-1_abc123", "client-1"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ClientId.Should().Be("client-2");
        result.Value.ClientSecret.Should().Be("new-secret");
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
    public async Task Handle_WhenFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .RegenerateUserPoolClientSecretAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<UserPoolClientDetail>>(new Error("regen boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new RegenerateUserPoolClientSecretCommand("eu-west-1_abc123", "client-1"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("regen boom");
        await _publisher.Received(1).PublishAsync(
            Arg.Is<Notification>(notification => notification.State == OperationState.Failed),
            Arg.Any<CancellationToken>());
        _activityLog.Received(1).Append(
            Arg.Is<ActivityEntry>(entry => entry.State == OperationState.Failed));
        _searchRefresh.DidNotReceive().RequestRefresh();
    }
}
