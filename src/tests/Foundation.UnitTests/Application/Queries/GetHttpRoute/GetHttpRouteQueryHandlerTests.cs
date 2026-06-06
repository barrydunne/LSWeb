using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGatewayV2;
using Foundation.Application.Queries.GetHttpRoute;
using Foundation.Domain.ApiGatewayV2;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetHttpRoute;

public class GetHttpRouteQueryHandlerTests
{
    private readonly IApiGatewayV2Client _client = Substitute.For<IApiGatewayV2Client>();

    private GetHttpRouteQueryHandler CreateSut()
        => new(_client, NullLogger<GetHttpRouteQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsRoute()
    {
        // Arrange
        var detail = new HttpRouteDetail(
            "route1", "GET /items", "integrations/int1", "JWT", "auth1", ["scope.read"], false);
        _client
            .GetRouteAsync("abc123", "route1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<HttpRouteDetail>>(detail));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetHttpRouteQuery("abc123", "route1"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var route = result.Value.Route;
        route.RouteId.Should().Be("route1");
        route.RouteKey.Should().Be("GET /items");
        route.Target.Should().Be("integrations/int1");
        route.AuthorizationType.Should().Be("JWT");
        route.AuthorizerId.Should().Be("auth1");
        route.AuthorizationScopes.Should().ContainSingle().Which.Should().Be("scope.read");
        route.ApiKeyRequired.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetRouteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<HttpRouteDetail>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetHttpRouteQuery("abc123", "route1"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
