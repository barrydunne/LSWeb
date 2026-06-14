using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Lambda;
using Foundation.Application.Queries.TestLambdaFunctionUrl;
using Foundation.Domain.Lambda;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.TestLambdaFunctionUrl;

public class TestLambdaFunctionUrlQueryHandlerTests
{
    private readonly ILambdaClient _client = Substitute.For<ILambdaClient>();

    private TestLambdaFunctionUrlQueryHandler CreateSut()
        => new(_client, NullLogger<TestLambdaFunctionUrlQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsTestResult()
    {
        // Arrange
        _client
            .TestFunctionUrlAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<LambdaFunctionUrlTest>>(
                new LambdaFunctionUrlTest(200, "{\"ok\":true}")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new TestLambdaFunctionUrlQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Test.StatusCode.Should().Be(200);
        result.Value.Test.Body.Should().Be("{\"ok\":true}");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .TestFunctionUrlAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<LambdaFunctionUrlTest>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new TestLambdaFunctionUrlQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
