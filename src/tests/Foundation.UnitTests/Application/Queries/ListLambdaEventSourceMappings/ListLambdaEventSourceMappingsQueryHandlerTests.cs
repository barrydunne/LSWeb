using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Lambda;
using Foundation.Application.Queries.ListLambdaEventSourceMappings;
using Foundation.Domain.Lambda;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListLambdaEventSourceMappings;

public class ListLambdaEventSourceMappingsQueryHandlerTests
{
    private readonly ILambdaClient _client = Substitute.For<ILambdaClient>();

    private ListLambdaEventSourceMappingsQueryHandler CreateSut()
        => new(_client, NullLogger<ListLambdaEventSourceMappingsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value) => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsMappingsOrderedByEventSourceArn()
    {
        // Arrange
        IReadOnlyList<LambdaEventSourceMapping> stored =
        [
            new("uuid-z", "arn:zeta", "arn:fn", "Enabled", 10, "2026-01-02T03:04:05Z"),
            new("uuid-a", "arn:alpha", "arn:fn", "Disabled", 5, "2026-01-01T00:00:00Z"),
        ];
        IReadOnlyList<LambdaS3Trigger> triggers =
        [
            new("arn:aws:s3:::zeta-bucket"),
            new("arn:aws:s3:::alpha-bucket"),
        ];
        _client
            .ListEventSourceMappingsAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(stored)));
        _client
            .ListS3TriggersAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(triggers)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListLambdaEventSourceMappingsQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Mappings.Select(_ => _.EventSourceArn).Should().ContainInOrder("arn:alpha", "arn:zeta");
        var first = result.Value.Mappings[0];
        first.Uuid.Should().Be("uuid-a");
        first.EventSourceArn.Should().Be("arn:alpha");
        first.FunctionArn.Should().Be("arn:fn");
        first.State.Should().Be("Disabled");
        first.BatchSize.Should().Be(5);
        first.LastModified.Should().Be("2026-01-01T00:00:00Z");
        result.Value.S3Triggers.Select(_ => _.BucketArn)
            .Should().ContainInOrder("arn:aws:s3:::alpha-bucket", "arn:aws:s3:::zeta-bucket");
    }

    [Fact]
    public async Task Handle_WhenClientFails_ReturnsError()
    {
        // Arrange
        _client
            .ListEventSourceMappingsAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<LambdaEventSourceMapping>>>(new Error("list boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListLambdaEventSourceMappingsQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("list boom");
    }

    [Fact]
    public async Task Handle_WhenS3TriggerClientFails_ReturnsError()
    {
        // Arrange
        IReadOnlyList<LambdaEventSourceMapping> stored = [];
        _client
            .ListEventSourceMappingsAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(stored)));
        _client
            .ListS3TriggersAsync("orders", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<LambdaS3Trigger>>>(new Error("policy boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListLambdaEventSourceMappingsQuery("orders"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("policy boom");
    }
}
