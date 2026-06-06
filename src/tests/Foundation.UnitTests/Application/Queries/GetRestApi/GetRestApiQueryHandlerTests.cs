using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGateway;
using Foundation.Application.Queries.GetRestApi;
using Foundation.Domain.ApiGateway;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetRestApi;

public class GetRestApiQueryHandlerTests
{
    private readonly IApiGatewayClient _client = Substitute.For<IApiGatewayClient>();

    private GetRestApiQueryHandler CreateSut()
        => new(_client, NullLogger<GetRestApiQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    private static RestApiDetail Detail()
        => new(
            "abc123",
            "orders",
            "Order API",
            "1.0",
            "HEADER",
            ["REGIONAL"],
            ["application/octet-stream"],
            DateTimeOffset.UnixEpoch);

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsApiAndForwardsId()
    {
        // Arrange
        _client
            .GetRestApiAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(Detail())));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetRestApiQuery("abc123"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.RestApi.Id.Should().Be("abc123");
        await _client.Received(1).GetRestApiAsync("abc123", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetRestApiAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<RestApiDetail>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetRestApiQuery("missing"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
