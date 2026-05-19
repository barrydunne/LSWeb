using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Lambda;
using Foundation.Application.Queries.ListLambdaFunctions;
using Foundation.Domain.Lambda;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListLambdaFunctions;

public class ListLambdaFunctionsQueryHandlerTests
{
    private readonly ILambdaClient _client = Substitute.For<ILambdaClient>();

    private ListLambdaFunctionsQueryHandler CreateSut()
        => new(_client, NullLogger<ListLambdaFunctionsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsFunctions()
    {
        // Arrange
        IReadOnlyList<LambdaFunctionSummary> functions =
        [
            new("process-orders", "dotnet8", "Order processor", "2026-01-02T03:04:05Z", 256, 30),
        ];
        _client
            .ListFunctionsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(functions)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new ListLambdaFunctionsQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Functions.Should().ContainSingle(_ => _.FunctionName == "process-orders");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListFunctionsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<LambdaFunctionSummary>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new ListLambdaFunctionsQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
