using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGatewayV2;
using Foundation.Application.Queries.ListHttpRoutes;
using Foundation.Domain.ApiGatewayV2;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListHttpRoutes;

public class ListHttpRoutesQueryHandlerTests
{
    private readonly IApiGatewayV2Client _client = Substitute.For<IApiGatewayV2Client>();

    private ListHttpRoutesQueryHandler CreateSut()
        => new(_client, NullLogger<ListHttpRoutesQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsRoutes()
    {
        // Arrange
        IReadOnlyList<HttpRouteSummary> routes =
        [
            new("route1", "GET /items", "integrations/int1", "NONE"),
        ];
        _client
            .ListRoutesAsync("abc123", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(routes)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListHttpRoutesQuery("abc123"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var route = result.Value.Routes.Should().ContainSingle().Subject;
        route.RouteId.Should().Be("route1");
        route.RouteKey.Should().Be("GET /items");
        route.Target.Should().Be("integrations/int1");
        route.AuthorizationType.Should().Be("NONE");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListRoutesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<HttpRouteSummary>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListHttpRoutesQuery("abc123"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
