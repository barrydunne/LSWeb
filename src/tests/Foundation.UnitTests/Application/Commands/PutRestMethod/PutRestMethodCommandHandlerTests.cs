using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.ApiGateway;
using Foundation.Application.Commands.PutRestMethod;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.ApiGateway;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.PutRestMethod;

public class PutRestMethodCommandHandlerTests
{
    private readonly IApiGatewayClient _client = Substitute.For<IApiGatewayClient>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private static PutRestMethodCommand BuildCommand()
        => new("api-1", "res-2", "GET", "NONE", null, false, []);

    private PutRestMethodCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh, NullLogger<PutRestMethodCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenPutSucceeds_PublishesSuccessAndRefreshesSearch()
    {
        // Arrange
        _client
            .PutMethodAsync(Arg.Any<RestMethodSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).PutMethodAsync(
            Arg.Is<RestMethodSpecification>(specification =>
                specification.RestApiId == "api-1"
                && specification.ResourceId == "res-2"
                && specification.HttpMethod == "GET"
                && specification.AuthorizationType == "NONE"),
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
    public async Task Handle_WhenPutFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .PutMethodAsync(Arg.Any<RestMethodSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("put boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("put boom");
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
            .PutMethodAsync(Arg.Any<RestMethodSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var command = new PutRestMethodCommand(
            "api-1", "res-2", "POST", "COGNITO_USER_POOLS", "auth-9", true, ["scope-a", "scope-b"]);
        var sut = CreateSut();

        // Act
        await sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await _client.Received(1).PutMethodAsync(
            Arg.Is<RestMethodSpecification>(specification =>
                specification.AuthorizationType == "COGNITO_USER_POOLS"
                && specification.AuthorizerId == "auth-9"
                && specification.ApiKeyRequired
                && specification.AuthorizationScopes.Count == 2),
            Arg.Any<CancellationToken>());
    }
}
