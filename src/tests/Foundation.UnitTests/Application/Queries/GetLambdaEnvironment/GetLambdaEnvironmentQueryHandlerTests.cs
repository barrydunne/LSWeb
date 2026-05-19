using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Diagnostics;
using Foundation.Application.Lambda;
using Foundation.Application.Queries.GetLambdaEnvironment;
using Foundation.Domain.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetLambdaEnvironment;

public class GetLambdaEnvironmentQueryHandlerTests
{
    private readonly ILambdaClient _client = Substitute.For<ILambdaClient>();
    private readonly IRedactionService _redaction = Substitute.For<IRedactionService>();

    private static Result<T> Ok<T>(T value) => value;

    private GetLambdaEnvironmentQueryHandler CreateSut()
        => new(_client, _redaction, NullLogger<GetLambdaEnvironmentQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsMaskedAndOrderedVariables()
    {
        // Arrange
        IReadOnlyDictionary<string, string> raw = new Dictionary<string, string>
        {
            ["REGION"] = "eu-west-1",
            ["API_KEY"] = "super-secret",
        };
        _client
            .GetEnvironmentAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(raw)));
        _redaction.CanReveal.Returns(true);
        _redaction
            .Resolve(Arg.Any<ConfigValue>(), Arg.Any<bool>())
            .Returns(call =>
            {
                var value = call.Arg<ConfigValue>();
                var reveal = call.Arg<bool>();
                return value.IsSensitive && !reveal ? ConfigValue.Mask : value.Value;
            });
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetLambdaEnvironmentQuery("orders", false),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.RevealAllowed.Should().BeTrue();
        result.Value.Variables.Should().HaveCount(2);

        var apiKey = result.Value.Variables[0];
        apiKey.Name.Should().Be("API_KEY");
        apiKey.Value.Should().Be(ConfigValue.Mask);
        apiKey.IsSensitive.Should().BeTrue();

        var region = result.Value.Variables[1];
        region.Name.Should().Be("REGION");
        region.Value.Should().Be("eu-west-1");
        region.IsSensitive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetEnvironmentAsync("missing", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyDictionary<string, string>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetLambdaEnvironmentQuery("missing", false),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
