using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Lambda;
using Foundation.Domain.Lambda;
using Foundation.Infrastructure.Search;

namespace Foundation.UnitTests.Infrastructure.Search;

public class LambdaResourceSourceTests
{
    private readonly ILambdaClient _client = Substitute.For<ILambdaClient>();

    private LambdaResourceSource CreateSut()
        => new(_client);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public void ServiceKey_IsLambda()
        => CreateSut().ServiceKey.Should().Be("lambda");

    [Fact]
    public async Task ListAsync_WhenClientSucceeds_MapsFunctionsToSearchEntries()
    {
        // Arrange
        IReadOnlyList<LambdaFunctionSummary> functions =
        [
            new("process-orders", "dotnet8", "Order processor", "2026-01-02T03:04:05Z", 256, 30),
            new("resize-images", "python3.12", "Image resizer", "2026-01-03T03:04:05Z", 512, 60),
        ];
        _client
            .ListFunctionsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(functions)));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        entries.Should().HaveCount(2);
        entries[0].ServiceKey.Should().Be("lambda");
        entries[0].ResourceId.Should().Be("process-orders");
        entries[0].DisplayName.Should().Be("process-orders");
        entries[0].Route.Should().Be("/services/lambda/process-orders");
    }

    [Fact]
    public async Task ListAsync_WhenClientFails_ReturnsEmptyList()
    {
        // Arrange
        _client
            .ListFunctionsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<LambdaFunctionSummary>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var entries = await sut.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        entries.Should().BeEmpty();
    }
}
