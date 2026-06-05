using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.EventBridge;
using Foundation.Application.Queries.ListScheduledRules;
using Foundation.Domain.EventBridge;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ListScheduledRules;

public class ListScheduledRulesQueryHandlerTests
{
    private readonly IEventBridgeClient _client = Substitute.For<IEventBridgeClient>();

    private ListScheduledRulesQueryHandler CreateSut()
        => new(_client, NullLogger<ListScheduledRulesQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsOnlyScheduledRules()
    {
        // Arrange
        IReadOnlyList<EventBridgeRule> rules =
        [
            new(
                "hourly-report",
                "arn:aws:events:eu-west-1:000000000000:rule/hourly-report",
                "default",
                "ENABLED",
                "Runs hourly",
                "rate(1 hour)"),
            new(
                "orders-rule",
                "arn:aws:events:eu-west-1:000000000000:rule/orders-rule",
                "default",
                "ENABLED",
                "Routes order events",
                null),
            new(
                "blank-schedule",
                "arn:aws:events:eu-west-1:000000000000:rule/blank-schedule",
                "default",
                "ENABLED",
                null,
                "   "),
        ];
        _client
            .ListRulesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(rules)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListScheduledRulesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Rules.Should().ContainSingle(_ => _.Name == "hourly-report");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .ListRulesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<EventBridgeRule>>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new ListScheduledRulesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
