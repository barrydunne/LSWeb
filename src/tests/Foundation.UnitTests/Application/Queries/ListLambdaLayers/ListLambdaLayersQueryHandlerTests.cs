using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Lambda;
using Foundation.Application.Queries.ListLambdaLayers;
using Foundation.Domain.Lambda;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListLambdaLayers;

public class ListLambdaLayersQueryHandlerTests
{
    private readonly ILambdaClient _client = Substitute.For<ILambdaClient>();

    private ListLambdaLayersQueryHandler CreateSut()
        => new(_client, NullLogger<ListLambdaLayersQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value) => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsLayersOrderedByArn()
    {
        // Arrange
        IReadOnlyList<LambdaLayer> stored =
        [
            new("arn:aws:lambda:eu-west-1:123456789012:layer:zeta:2", "zeta", "2"),
            new("arn:aws:lambda:eu-west-1:123456789012:layer:alpha:5", "alpha", "5"),
        ];
        _client
            .ListLayersAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(stored)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListLambdaLayersQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Layers.Select(_ => _.Name).Should().ContainInOrder("alpha", "zeta");
        var first = result.Value.Layers[0];
        first.Arn.Should().Be("arn:aws:lambda:eu-west-1:123456789012:layer:alpha:5");
        first.Name.Should().Be("alpha");
        first.Version.Should().Be("5");
    }

    [Fact]
    public async Task Handle_WhenClientFails_ReturnsError()
    {
        // Arrange
        _client
            .ListLayersAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<LambdaLayer>>>(new Error("list boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListLambdaLayersQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("list boom");
    }
}
