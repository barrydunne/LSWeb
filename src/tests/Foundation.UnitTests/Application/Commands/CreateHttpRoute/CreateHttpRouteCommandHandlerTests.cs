using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.ApiGatewayV2;
using Foundation.Application.Commands.CreateHttpRoute;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.ApiGatewayV2;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.CreateHttpRoute;

public class CreateHttpRouteCommandHandlerTests
{
    private readonly IApiGatewayV2Client _client = Substitute.For<IApiGatewayV2Client>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private static CreateHttpRouteCommand BuildCommand()
        => new("abc123", "GET /items", "integrations/int1", "NONE", null, []);

    private CreateHttpRouteCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh, NullLogger<CreateHttpRouteCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenCreateSucceeds_PublishesSuccessAndRefreshesSearch()
    {
        // Arrange
        _client
            .CreateRouteAsync(Arg.Any<HttpRouteSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("route1"));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("route1");
        await _client.Received(1).CreateRouteAsync(
            Arg.Is<HttpRouteSpecification>(specification =>
                specification.ApiId == "abc123"
                && specification.RouteId == null
                && specification.RouteKey == "GET /items"),
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
            .CreateRouteAsync(Arg.Any<HttpRouteSpecification>(), Arg.Any<CancellationToken>())
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
            .CreateRouteAsync(Arg.Any<HttpRouteSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<string>>("route1"));
        var command = new CreateHttpRouteCommand(
            "api9", "POST /orders", "integrations/int9", "JWT", "auth9", ["scope.read"]);
        var sut = CreateSut();

        // Act
        await sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await _client.Received(1).CreateRouteAsync(
            Arg.Is<HttpRouteSpecification>(specification =>
                specification.ApiId == "api9"
                && specification.RouteId == null
                && specification.RouteKey == "POST /orders"
                && specification.Target == "integrations/int9"
                && specification.AuthorizationType == "JWT"
                && specification.AuthorizerId == "auth9"
                && specification.AuthorizationScopes.Contains("scope.read")),
            Arg.Any<CancellationToken>());
    }
}
