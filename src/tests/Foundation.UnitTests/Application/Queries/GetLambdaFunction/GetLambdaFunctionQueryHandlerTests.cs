using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Lambda;
using Foundation.Application.Queries.GetLambdaFunction;
using Foundation.Domain.Lambda;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetLambdaFunction;

public class GetLambdaFunctionQueryHandlerTests
{
    private readonly ILambdaClient _client = Substitute.For<ILambdaClient>();

    private GetLambdaFunctionQueryHandler CreateSut()
        => new(_client, NullLogger<GetLambdaFunctionQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsFunctionDetail()
    {
        // Arrange
        var detail = new LambdaFunctionDetail(
            "process-orders",
            "arn:aws:lambda:eu-west-1:000000000000:function:process-orders",
            "dotnet8",
            "Orders::Handler",
            "Order processor",
            "2026-01-02T03:04:05Z",
            256,
            30,
            "arn:aws:iam::000000000000:role/lambda-orders");
        _client
            .GetFunctionAsync("process-orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<LambdaFunctionDetail>>(detail));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetLambdaFunctionQuery("process-orders"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Function.FunctionName.Should().Be("process-orders");
        result.Value.Function.Handler.Should().Be("Orders::Handler");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetFunctionAsync("missing", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<LambdaFunctionDetail>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetLambdaFunctionQuery("missing"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
