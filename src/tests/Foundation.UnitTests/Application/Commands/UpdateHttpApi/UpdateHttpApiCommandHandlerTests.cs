using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.ApiGatewayV2;
using Foundation.Application.Commands.UpdateHttpApi;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.ApiGatewayV2;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.UpdateHttpApi;

public class UpdateHttpApiCommandHandlerTests
{
    private readonly IApiGatewayV2Client _client = Substitute.For<IApiGatewayV2Client>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly IActivityLog _activityLog = Substitute.For<IActivityLog>();
    private readonly ISearchRefreshTrigger _searchRefresh = Substitute.For<ISearchRefreshTrigger>();

    private static UpdateHttpApiCommand BuildCommand()
        => new("abc123", "orders", "HTTP", "Order API", "1.0", null);

    private UpdateHttpApiCommandHandler CreateSut()
        => new(_client, _publisher, _activityLog, _searchRefresh, NullLogger<UpdateHttpApiCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenUpdateSucceeds_PublishesSuccessAndRefreshesSearch()
    {
        // Arrange
        _client
            .UpdateApiAsync(Arg.Any<HttpApiSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(BuildCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _client.Received(1).UpdateApiAsync(
            Arg.Is<HttpApiSpecification>(specification =>
                specification.ApiId == "abc123" && specification.Name == "orders"),
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
    public async Task Handle_WhenUpdateFails_PublishesFailureAndReturnsError()
    {
        // Arrange
        _client
            .UpdateApiAsync(Arg.Any<HttpApiSpecification>(), Arg.Any<CancellationToken>())
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
        _searchRefresh.DidNotReceive().RequestRefresh();
    }

    [Fact]
    public async Task Handle_MapsAllCommandFieldsOntoSpecification()
    {
        // Arrange
        _client
            .UpdateApiAsync(Arg.Any<HttpApiSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var command = new UpdateHttpApiCommand(
            "abc123", "events", "WEBSOCKET", "Event API", "2.0", "$request.body.action",
            new HttpApiCorsConfiguration(true, ["content-type"], ["GET"], ["*"], [], 600));
        var sut = CreateSut();

        // Act
        await sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await _client.Received(1).UpdateApiAsync(
            Arg.Is<HttpApiSpecification>(specification =>
                specification.ApiId == "abc123"
                && specification.Name == "events"
                && specification.ProtocolType == "WEBSOCKET"
                && specification.Description == "Event API"
                && specification.Version == "2.0"
                && specification.RouteSelectionExpression == "$request.body.action"
                && specification.CorsConfiguration!.AllowOrigins[0] == "*"
                && specification.CorsConfiguration.AllowMethods[0] == "GET"
                && specification.CorsConfiguration.MaxAge == 600),
            Arg.Any<CancellationToken>());
    }
}
