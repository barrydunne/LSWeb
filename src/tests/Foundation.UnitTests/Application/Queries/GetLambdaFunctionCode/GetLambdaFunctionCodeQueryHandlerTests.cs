using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Lambda;
using Foundation.Application.Queries.GetLambdaFunctionCode;
using Foundation.Domain.Lambda;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetLambdaFunctionCode;

public class GetLambdaFunctionCodeQueryHandlerTests
{
    private readonly ILambdaClient _client = Substitute.For<ILambdaClient>();

    private GetLambdaFunctionCodeQueryHandler CreateSut()
        => new(_client, NullLogger<GetLambdaFunctionCodeQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsFunctionCode()
    {
        // Arrange
        var code = new LambdaFunctionCode(
            "process-orders",
            "dotnet8",
            "Orders::Handler",
            "Zip",
            2048,
            "abc123=",
            "S3",
            "https://localstack/download.zip",
            string.Empty);
        _client
            .GetFunctionCodeAsync("process-orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<LambdaFunctionCode>>(code));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetLambdaFunctionCodeQuery("process-orders"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Code.FunctionName.Should().Be("process-orders");
        result.Value.Code.Handler.Should().Be("Orders::Handler");
        result.Value.Code.PackageType.Should().Be("Zip");
        result.Value.Code.CodeSize.Should().Be(2048);
        result.Value.Code.Location.Should().Be("https://localstack/download.zip");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetFunctionCodeAsync("missing", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<LambdaFunctionCode>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetLambdaFunctionCodeQuery("missing"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
