using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.EventBridge;
using Foundation.Application.Queries.GetEventBridgeRule;
using Foundation.Domain.EventBridge;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.GetEventBridgeRule;

public class GetEventBridgeRuleQueryHandlerTests
{
    private readonly IEventBridgeClient _client = Substitute.For<IEventBridgeClient>();

    private static Result<EventBridgeRuleDetail> Ok(EventBridgeRuleDetail detail)
        => detail;

    private GetEventBridgeRuleQueryHandler CreateSut()
        => new(_client, NullLogger<GetEventBridgeRuleQueryHandler>.Instance);

    [Fact]
    public async Task Handle_WhenClientSucceeds_ReturnsRuleDetailWithPattern()
    {
        // Arrange
        var detail = new EventBridgeRuleDetail(
            "orders-rule",
            "arn:aws:events:eu-west-1:000000000000:rule/orders-rule",
            "default",
            "ENABLED",
            null,
            "desc",
            null,
            null,
            "{\"source\":[\"my.app\"]}");
        _client
            .DescribeRuleAsync("orders-rule", null, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Ok(detail)));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetEventBridgeRuleQuery("orders-rule", null), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Rule.EventPattern.Should().Be("{\"source\":[\"my.app\"]}");
    }

    [Fact]
    public async Task Handle_WhenClientFails_ReturnsError()
    {
        // Arrange
        _client
            .DescribeRuleAsync("missing", null, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<EventBridgeRuleDetail>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(
            new GetEventBridgeRuleQuery("missing", null), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Message.Should().Be("boom");
    }
}
