using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGateway;
using Foundation.Application.Queries.GetRestMethod;
using Foundation.Domain.ApiGateway;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetRestMethod;

public class GetRestMethodQueryHandlerTests
{
    private readonly IApiGatewayClient _client = Substitute.For<IApiGatewayClient>();

    private GetRestMethodQueryHandler CreateSut()
        => new(_client, NullLogger<GetRestMethodQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsMethod()
    {
        // Arrange
        var method = new RestMethodDetail("res-2", "GET", "NONE", null, false, []);
        _client
            .GetMethodAsync("api-1", "res-2", "GET", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<RestMethodDetail>>(method));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetRestMethodQuery("api-1", "res-2", "GET"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Method.HttpMethod.Should().Be("GET");
        result.Value.Method.ResourceId.Should().Be("res-2");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetMethodAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<RestMethodDetail>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetRestMethodQuery("api-1", "res-2", "GET"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
