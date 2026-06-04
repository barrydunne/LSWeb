using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGateway;
using Foundation.Application.Queries.ListRestApis;
using Foundation.Domain.ApiGateway;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListRestApis;

public class ListRestApisQueryHandlerTests
{
    private readonly IApiGatewayClient _client = Substitute.For<IApiGatewayClient>();

    private ListRestApisQueryHandler CreateSut()
        => new(_client, NullLogger<ListRestApisQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsRestApis()
    {
        // Arrange
        IReadOnlyList<RestApi> restApis =
        [
            new("api-1", "orders-api", "Orders API", DateTimeOffset.UnixEpoch),
        ];
        _client
            .ListRestApisAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(restApis)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListRestApisQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.RestApis.Should().ContainSingle(_ => _.Name == "orders-api");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListRestApisAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<RestApi>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListRestApisQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
