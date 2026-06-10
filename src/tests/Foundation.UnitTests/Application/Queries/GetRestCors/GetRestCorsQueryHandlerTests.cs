using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGateway;
using Foundation.Application.Queries.GetRestCors;
using Foundation.Domain.ApiGateway;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetRestCors;

public class GetRestCorsQueryHandlerTests
{
    private readonly IApiGatewayClient _client = Substitute.For<IApiGatewayClient>();

    private GetRestCorsQueryHandler CreateSut()
        => new(_client, NullLogger<GetRestCorsQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsCors()
    {
        // Arrange
        var configuration = new RestCorsConfiguration(
            "resource-1",
            true,
            ["*"],
            ["GET", "POST"],
            ["Content-Type"]);
        _client
            .GetCorsAsync("api-1", "resource-1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<RestCorsConfiguration>>(configuration));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetRestCorsQuery("api-1", "resource-1"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Cors.ResourceId.Should().Be("resource-1");
        result.Value.Cors.Enabled.Should().BeTrue();
        result.Value.Cors.AllowOrigins.Should().ContainSingle().Which.Should().Be("*");
        result.Value.Cors.AllowMethods.Should().BeEquivalentTo("GET", "POST");
        result.Value.Cors.AllowHeaders.Should().ContainSingle().Which.Should().Be("Content-Type");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetCorsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<RestCorsConfiguration>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetRestCorsQuery("api-1", "resource-1"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
