using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGatewayV2;
using Foundation.Domain.ApiGatewayV2;
using Foundation.Infrastructure.Search;

namespace Foundation.UnitTests.Infrastructure.Search;

public class ApiGatewayV2ResourceSourceTests
{
    private readonly IApiGatewayV2Client _client = Substitute.For<IApiGatewayV2Client>();

    private ApiGatewayV2ResourceSource CreateSut()
        => new(_client);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public void ServiceKey_IsApiGatewayV2()
        => CreateSut().ServiceKey.Should().Be("apigatewayv2");

    [Fact]
    public async Task ListAsync_WhenClientSucceeds_MapsApisToSearchEntries()
    {
        // Arrange
        IReadOnlyList<HttpApiSummary> apis =
        [
            new("abc 123", "orders", "HTTP", "https://abc.execute-api", DateTimeOffset.UnixEpoch),
        ];
        _client
            .ListApisAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(apis)));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        var entry = entries.Should().ContainSingle().Subject;
        entry.ServiceKey.Should().Be("apigatewayv2");
        entry.ResourceId.Should().Be("abc 123");
        entry.DisplayName.Should().Be("orders");
        entry.Route.Should().Be("/services/apigatewayv2/abc%20123");
    }

    [Fact]
    public async Task ListAsync_WhenClientFails_ReturnsEmptyList()
    {
        // Arrange
        _client
            .ListApisAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<HttpApiSummary>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        entries.Should().BeEmpty();
    }
}
