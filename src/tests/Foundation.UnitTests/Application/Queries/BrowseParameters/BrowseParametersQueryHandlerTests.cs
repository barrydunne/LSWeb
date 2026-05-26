using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.BrowseParameters;
using Foundation.Application.Ssm;
using Foundation.Domain.Ssm;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.BrowseParameters;

public class BrowseParametersQueryHandlerTests
{
    private readonly ISsmClient _client = Substitute.For<ISsmClient>();

    private BrowseParametersQueryHandler CreateSut()
        => new(_client, NullLogger<BrowseParametersQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsParameters()
    {
        // Arrange
        IReadOnlyList<Parameter> parameters =
        [
            new("/app/config/key", "String", 1, null, "arn:/app/config/key"),
        ];
        _client
            .GetParametersByPathAsync("/app", true, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(parameters)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new BrowseParametersQuery("/app", true), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Path.Should().Be("/app");
        result.Value.Parameters.Should().ContainSingle(_ => _.Name == "/app/config/key");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .GetParametersByPathAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<Parameter>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new BrowseParametersQuery("/", false), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
