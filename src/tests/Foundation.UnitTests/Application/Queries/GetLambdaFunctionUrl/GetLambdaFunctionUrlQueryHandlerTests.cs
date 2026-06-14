using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Lambda;
using Foundation.Application.Queries.GetLambdaFunctionUrl;
using Foundation.Domain.Lambda;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetLambdaFunctionUrl;

public class GetLambdaFunctionUrlQueryHandlerTests
{
    private readonly ILambdaClient _client = Substitute.For<ILambdaClient>();

    private GetLambdaFunctionUrlQueryHandler CreateSut()
        => new(_client, NullLogger<GetLambdaFunctionUrlQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenClientReturnsUrl_ReturnsUrl()
    {
        // Arrange
        var url = new LambdaFunctionUrl("https://abc.lambda-url.eu-west-1.on.aws/", "NONE", "t1", "t2");
        _client
            .GetFunctionUrlAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<LambdaFunctionUrl?>>(url));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetLambdaFunctionUrlQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Url!.FunctionUrl.Should().Be("https://abc.lambda-url.eu-west-1.on.aws/");
        result.Value.Url.AuthType.Should().Be("NONE");
    }

    [Fact]
    public async Task Handle_WhenNoUrlConfigured_ReturnsNull()
    {
        // Arrange
        _client
            .GetFunctionUrlAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<LambdaFunctionUrl?>>((LambdaFunctionUrl?)null));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetLambdaFunctionUrlQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Url.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetFunctionUrlAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<LambdaFunctionUrl?>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetLambdaFunctionUrlQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
