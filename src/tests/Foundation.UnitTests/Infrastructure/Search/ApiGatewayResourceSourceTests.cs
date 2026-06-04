using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGateway;
using Foundation.Domain.ApiGateway;
using Foundation.Infrastructure.Search;

namespace Foundation.UnitTests.Infrastructure.Search;

public class ApiGatewayResourceSourceTests
{
    private readonly IApiGatewayClient _client = Substitute.For<IApiGatewayClient>();

    private ApiGatewayResourceSource CreateSut()
        => new(_client);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public void ServiceKey_IsApiGateway()
        => CreateSut().ServiceKey.Should().Be("apigateway");

    [Fact]
    public async Task ListAsync_WhenClientSucceeds_MapsRestApisToSearchEntries()
    {
        // Arrange
        IReadOnlyList<RestApi> restApis =
        [
            new("api 1", "orders-api", null, null),
        ];
        _client
            .ListRestApisAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(restApis)));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        var entry = entries.Should().ContainSingle().Subject;
        entry.ServiceKey.Should().Be("apigateway");
        entry.ResourceId.Should().Be("api 1");
        entry.DisplayName.Should().Be("orders-api");
        entry.Route.Should().Be("/services/apigateway/api%201");
    }

    [Fact]
    public async Task ListAsync_WhenClientFails_ReturnsEmptyList()
    {
        // Arrange
        _client
            .ListRestApisAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<RestApi>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        entries.Should().BeEmpty();
    }
}
