using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGateway;
using Foundation.Application.Queries.ListRestResources;
using Foundation.Domain.ApiGateway;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListRestResources;

public class ListRestResourcesQueryHandlerTests
{
    private readonly IApiGatewayClient _client = Substitute.For<IApiGatewayClient>();

    private ListRestResourcesQueryHandler CreateSut()
        => new(_client, NullLogger<ListRestResourcesQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsResources()
    {
        // Arrange
        IReadOnlyList<RestResourceSummary> resources =
        [
            new("res-1", null, null, "/", []),
            new("res-2", "res-1", "items", "/items", ["GET", "POST"]),
        ];
        _client
            .ListResourcesAsync("api-1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(resources)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListRestResourcesQuery("api-1"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Resources.Should().HaveCount(2);
        result.Value.Resources.Should().Contain(_ => _.PathPart == "items");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListResourcesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<RestResourceSummary>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListRestResourcesQuery("api-1"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
