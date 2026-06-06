using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGatewayV2;
using Foundation.Application.Queries.GetHttpApi;
using Foundation.Domain.ApiGatewayV2;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetHttpApi;

public class GetHttpApiQueryHandlerTests
{
    private readonly IApiGatewayV2Client _client = Substitute.For<IApiGatewayV2Client>();

    private GetHttpApiQueryHandler CreateSut()
        => new(_client, NullLogger<GetHttpApiQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    private static HttpApiDetail Detail()
        => new(
            "abc123",
            "orders",
            "HTTP",
            "https://abc123.execute-api",
            "Order API",
            "1.0",
            "$request.method $request.path",
            null,
            DateTimeOffset.UnixEpoch);

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsApiAndForwardsId()
    {
        // Arrange
        _client
            .GetApiAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(Detail())));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetHttpApiQuery("abc123"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Api.ApiId.Should().Be("abc123");
        await _client.Received(1).GetApiAsync("abc123", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetApiAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<HttpApiDetail>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetHttpApiQuery("missing"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
