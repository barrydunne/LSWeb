using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Commands.PutEventBridgeEvent;
using Foundation.Application.Queries.ListEventBridgeRules;
using Foundation.Application.Queries.ListEventBridgeTargets;
using Foundation.Domain.EventBridge;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Foundation.UnitTests.Controllers;

public class EventBridgeControllerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ILogger<EventBridgeController> _logger =
        Substitute.For<ILogger<EventBridgeController>>();

    private EventBridgeController CreateSut()
        => new(_sender, _logger);

    [Fact]
    public async Task ListRules_WhenQuerySucceeds_ReturnsOkWithRules()
    {
        // Arrange
        IReadOnlyList<EventBridgeRule> rules =
        [
            new(
                "orders-rule",
                "arn:aws:events:eu-west-1:000000000000:rule/orders-rule",
                "default",
                "ENABLED",
                "Routes order events",
                "rate(5 minutes)"),
        ];
        _sender
            .Send(Arg.Any<ListEventBridgeRulesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListEventBridgeRulesQueryResult>>(
                new ListEventBridgeRulesQueryResult(rules)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListRules(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<RuleListResponse>>().Subject;
        var rule = ok.Value!.Rules.Should().ContainSingle().Subject;
        rule.Name.Should().Be("orders-rule");
        rule.Arn.Should().Be("arn:aws:events:eu-west-1:000000000000:rule/orders-rule");
        rule.EventBusName.Should().Be("default");
        rule.State.Should().Be("ENABLED");
        rule.Description.Should().Be("Routes order events");
        rule.ScheduleExpression.Should().Be("rate(5 minutes)");
    }

    [Fact]
    public async Task ListRules_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListEventBridgeRulesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListEventBridgeRulesQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListRules(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task ListTargets_WhenQuerySucceeds_ReturnsOkWithTargetsAndForwardsRule()
    {
        // Arrange
        IReadOnlyList<EventBridgeTarget> targets =
        [
            new("target-1", "arn:aws:lambda:eu-west-1:000000000000:function:orders-handler"),
        ];
        ListEventBridgeTargetsQuery? captured = null;
        _sender
            .Send(
                Arg.Do<ListEventBridgeTargetsQuery>(query => captured = query),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListEventBridgeTargetsQueryResult>>(
                new ListEventBridgeTargetsQueryResult(targets)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListTargets("orders-rule", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<TargetListResponse>>().Subject;
        var target = ok.Value!.Targets.Should().ContainSingle().Subject;
        target.Id.Should().Be("target-1");
        target.Arn.Should().Be("arn:aws:lambda:eu-west-1:000000000000:function:orders-handler");
        captured.Should().NotBeNull();
        captured!.RuleName.Should().Be("orders-rule");
    }

    [Fact]
    public async Task ListTargets_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListEventBridgeTargetsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListEventBridgeTargetsQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListTargets("missing", TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task PutEvent_WhenCommandSucceeds_ReturnsOkWithOutcomeAndForwardsRequest()
    {
        // Arrange
        PutEventBridgeEventCommand? captured = null;
        _sender
            .Send(
                Arg.Do<PutEventBridgeEventCommand>(command => captured = command),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<EventBridgePutResult>>(
                new EventBridgePutResult("event-1", 0, null, null)));
        var sut = CreateSut();

        // Act
        var result = await sut.PutEvent(
            new PutEventRequest("orders.service", "OrderPlaced", "{\"orderId\":\"abc\"}", "orders-bus"),
            TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<PutEventResponse>>().Subject;
        ok.Value!.Accepted.Should().BeTrue();
        ok.Value.EventId.Should().Be("event-1");
        ok.Value.ErrorCode.Should().BeNull();
        ok.Value.ErrorMessage.Should().BeNull();
        captured.Should().NotBeNull();
        captured!.Source.Should().Be("orders.service");
        captured.DetailType.Should().Be("OrderPlaced");
        captured.Detail.Should().Be("{\"orderId\":\"abc\"}");
        captured.EventBusName.Should().Be("orders-bus");
    }

    [Fact]
    public async Task PutEvent_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<PutEventBridgeEventCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<EventBridgePutResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.PutEvent(
            new PutEventRequest("orders.service", "OrderPlaced", "{\"orderId\":\"abc\"}", null),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }
}
