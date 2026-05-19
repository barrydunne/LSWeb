using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Lambda;
using Foundation.Application.Queries.ListLambdaLogEvents;
using Foundation.Domain.Lambda;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListLambdaLogEvents;

public class ListLambdaLogEventsQueryHandlerTests
{
    private readonly ILambdaClient _client = Substitute.For<ILambdaClient>();

    private ListLambdaLogEventsQueryHandler CreateSut()
        => new(_client, NullLogger<ListLambdaLogEventsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value) => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsEventsOrderedByTimestampWithLogGroupName()
    {
        // Arrange
        IReadOnlyList<LambdaLogEvent> stored =
        [
            new("2026-01-02T03:04:05.0000000+00:00", "second", "stream-b"),
            new("2026-01-01T00:00:00.0000000+00:00", "first", "stream-a"),
        ];
        _client
            .GetRecentLogEventsAsync("orders", Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(stored)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListLambdaLogEventsQuery("orders", 50), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.LogGroupName.Should().Be("/aws/lambda/orders");
        result.Value.Events.Select(_ => _.Message).Should().ContainInOrder("first", "second");
        var first = result.Value.Events[0];
        first.Timestamp.Should().Be("2026-01-01T00:00:00.0000000+00:00");
        first.Message.Should().Be("first");
        first.LogStreamName.Should().Be("stream-a");
    }

    [Fact]
    public async Task Handle_WhenLimitIsZero_UsesDefaultLimit()
    {
        // Arrange
        _client
            .GetRecentLogEventsAsync("orders", Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok<IReadOnlyList<LambdaLogEvent>>([])));
        var sut = CreateSut();

        // Act
        await sut.Handle(new ListLambdaLogEventsQuery("orders", 0), TestContext.Current.CancellationToken);

        // Assert
        await _client.Received(1).GetRecentLogEventsAsync("orders", 100, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLimitExceedsMaximum_ClampsToMaximum()
    {
        // Arrange
        _client
            .GetRecentLogEventsAsync("orders", Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok<IReadOnlyList<LambdaLogEvent>>([])));
        var sut = CreateSut();

        // Act
        await sut.Handle(new ListLambdaLogEventsQuery("orders", 5000), TestContext.Current.CancellationToken);

        // Assert
        await _client.Received(1).GetRecentLogEventsAsync("orders", 1000, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLimitIsWithinRange_PassesLimitThrough()
    {
        // Arrange
        _client
            .GetRecentLogEventsAsync("orders", Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok<IReadOnlyList<LambdaLogEvent>>([])));
        var sut = CreateSut();

        // Act
        await sut.Handle(new ListLambdaLogEventsQuery("orders", 250), TestContext.Current.CancellationToken);

        // Assert
        await _client.Received(1).GetRecentLogEventsAsync("orders", 250, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenClientFails_ReturnsError()
    {
        // Arrange
        _client
            .GetRecentLogEventsAsync("orders", Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<LambdaLogEvent>>>(new Error("logs boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListLambdaLogEventsQuery("orders", 50), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("logs boom");
    }
}
