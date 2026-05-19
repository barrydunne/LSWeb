using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Lambda;
using Foundation.Application.Queries.GetLambdaInvocationInsights;
using Foundation.Domain.Lambda;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetLambdaInvocationInsights;

public class GetLambdaInvocationInsightsQueryHandlerTests
{
    private readonly ILambdaClient _client = Substitute.For<ILambdaClient>();

    private GetLambdaInvocationInsightsQueryHandler CreateSut()
        => new(_client, NullLogger<GetLambdaInvocationInsightsQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value) => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_DerivesInsightsWithLogGroupName()
    {
        // Arrange
        IReadOnlyList<LambdaLogEvent> stored =
        [
            new("2026-01-01T00:00:00.0000000+00:00", "REPORT RequestId: abc Duration: 12.50 ms Billed Duration: 13 ms", "stream-a"),
        ];
        _client
            .GetRecentLogEventsAsync("orders", Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(stored)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetLambdaInvocationInsightsQuery("orders", 50), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.LogGroupName.Should().Be("/aws/lambda/orders");
        result.Value.Insights.Metrics.InvocationCount.Should().Be(1);
        result.Value.Insights.RecentInvocations.Should().ContainSingle()
            .Which.RequestId.Should().Be("abc");
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
        await sut.Handle(new GetLambdaInvocationInsightsQuery("orders", 0), TestContext.Current.CancellationToken);

        // Assert
        await _client.Received(1).GetRecentLogEventsAsync("orders", 200, Arg.Any<CancellationToken>());
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
        await sut.Handle(new GetLambdaInvocationInsightsQuery("orders", 5000), TestContext.Current.CancellationToken);

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
        await sut.Handle(new GetLambdaInvocationInsightsQuery("orders", 250), TestContext.Current.CancellationToken);

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
            new GetLambdaInvocationInsightsQuery("orders", 50), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("logs boom");
    }
}
