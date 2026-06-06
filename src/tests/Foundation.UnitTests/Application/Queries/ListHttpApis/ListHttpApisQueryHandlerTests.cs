using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGatewayV2;
using Foundation.Application.Queries.ListHttpApis;
using Foundation.Domain.ApiGatewayV2;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListHttpApis;

public class ListHttpApisQueryHandlerTests
{
    private readonly IApiGatewayV2Client _client = Substitute.For<IApiGatewayV2Client>();

    private ListHttpApisQueryHandler CreateSut()
        => new(_client, NullLogger<ListHttpApisQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsApis()
    {
        // Arrange
        IReadOnlyList<HttpApiSummary> apis =
        [
            new("abc123", "orders", "HTTP", "https://abc123.execute-api", DateTimeOffset.UnixEpoch),
        ];
        _client
            .ListApisAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(apis)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListHttpApisQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Apis.Should().ContainSingle(_ => _.ApiId == "abc123");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListApisAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<HttpApiSummary>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListHttpApisQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
