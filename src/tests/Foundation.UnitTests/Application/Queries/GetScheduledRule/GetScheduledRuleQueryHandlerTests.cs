using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.EventBridge;
using Foundation.Application.Queries.GetScheduledRule;
using Foundation.Domain.EventBridge;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetScheduledRule;

public class GetScheduledRuleQueryHandlerTests
{
    private readonly IEventBridgeClient _client = Substitute.For<IEventBridgeClient>();

    private GetScheduledRuleQueryHandler CreateSut()
        => new(_client, NullLogger<GetScheduledRuleQueryHandler>.Instance);

    private static Result<T> Ok<T>(T value)
        => value;

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsRuleDetail()
    {
        // Arrange
        var detail = new EventBridgeRuleDetail(
            "hourly-report",
            "arn:aws:events:eu-west-1:000000000000:rule/hourly-report",
            "default",
            "ENABLED",
            "rate(1 hour)",
            "Runs hourly",
            "arn:aws:iam::000000000000:role/scheduler",
            null,
            null);
        _client
            .DescribeRuleAsync("hourly-report", null, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(detail)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetScheduledRuleQuery("hourly-report", null),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Rule.Name.Should().Be("hourly-report");
        result.Value.Rule.ScheduleExpression.Should().Be("rate(1 hour)");
    }

    [Fact]
    public async Task Handle_WhenClientFails_PropagatesError()
    {
        // Arrange
        _client
            .DescribeRuleAsync("missing", "custom-bus", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<EventBridgeRuleDetail>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetScheduledRuleQuery("missing", "custom-bus"),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
