using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Commands.DeleteScheduledRule;
using Foundation.Application.Commands.PutEventBridgeEvent;
using Foundation.Application.Commands.PutScheduledRule;
using Foundation.Application.Commands.PutScheduledRuleTargets;
using Foundation.Application.Commands.RemoveScheduledRuleTargets;
using Foundation.Application.Commands.SetScheduledRuleState;
using Foundation.Application.Queries.GetScheduledRule;
using Foundation.Application.Queries.ListEventBridgeRules;
using Foundation.Application.Queries.ListEventBridgeTargets;
using Foundation.Application.Queries.ListScheduledRules;
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
    public async Task ListScheduledRules_WhenQuerySucceeds_ReturnsOkWithRules()
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
        ];
        _sender
            .Send(Arg.Any<ListScheduledRulesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListScheduledRulesQueryResult>>(
                new ListScheduledRulesQueryResult(rules)));
        var sut = CreateSut();

        // Act
        var result = await sut.ListScheduledRules(TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<ScheduledRuleListResponse>>().Subject;
        var rule = ok.Value!.Rules.Should().ContainSingle().Subject;
        rule.Name.Should().Be("hourly-report");
        rule.Arn.Should().Be("arn:aws:events:eu-west-1:000000000000:rule/hourly-report");
        rule.EventBusName.Should().Be("default");
        rule.State.Should().Be("ENABLED");
        rule.Description.Should().Be("Runs hourly");
        rule.ScheduleExpression.Should().Be("rate(1 hour)");
    }

    [Fact]
    public async Task ListScheduledRules_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ListScheduledRulesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ListScheduledRulesQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.ListScheduledRules(TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task GetScheduledRule_WhenQuerySucceeds_ReturnsOkWithDetailAndForwardsArguments()
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
            "events.amazonaws.com");
        GetScheduledRuleQuery? captured = null;
        _sender
            .Send(
                Arg.Do<GetScheduledRuleQuery>(query => captured = query),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetScheduledRuleQueryResult>>(
                new GetScheduledRuleQueryResult(detail)));
        var sut = CreateSut();

        // Act
        var result = await sut.GetScheduledRule(
            "hourly-report", "custom-bus", TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<ScheduledRuleDetailResponse>>().Subject;
        ok.Value!.Name.Should().Be("hourly-report");
        ok.Value.Arn.Should().Be("arn:aws:events:eu-west-1:000000000000:rule/hourly-report");
        ok.Value.EventBusName.Should().Be("default");
        ok.Value.State.Should().Be("ENABLED");
        ok.Value.ScheduleExpression.Should().Be("rate(1 hour)");
        ok.Value.Description.Should().Be("Runs hourly");
        ok.Value.RoleArn.Should().Be("arn:aws:iam::000000000000:role/scheduler");
        ok.Value.ManagedBy.Should().Be("events.amazonaws.com");
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("hourly-report");
        captured.EventBusName.Should().Be("custom-bus");
    }

    [Fact]
    public async Task GetScheduledRule_WhenQueryFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<GetScheduledRuleQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<GetScheduledRuleQueryResult>>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.GetScheduledRule(
            "missing", null, TestContext.Current.CancellationToken);

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

    [Fact]
    public async Task CreateScheduledRule_WhenCommandSucceeds_ReturnsCreatedAndForwardsRequest()
    {
        // Arrange
        PutScheduledRuleCommand? captured = null;
        _sender
            .Send(
                Arg.Do<PutScheduledRuleCommand>(command => captured = command),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateScheduledRule(
            new ScheduledRulePutRequest("daily-rule", "rate(5 minutes)", "ENABLED", "nightly", "custom-bus"),
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<Created>();
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("daily-rule");
        captured.ScheduleExpression.Should().Be("rate(5 minutes)");
        captured.State.Should().Be("ENABLED");
        captured.Description.Should().Be("nightly");
        captured.EventBusName.Should().Be("custom-bus");
    }

    [Fact]
    public async Task CreateScheduledRule_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<PutScheduledRuleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.CreateScheduledRule(
            new ScheduledRulePutRequest("daily-rule", "rate(5 minutes)", "ENABLED", null, null),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task UpdateScheduledRule_WhenCommandSucceeds_ReturnsNoContentAndForwardsRequest()
    {
        // Arrange
        PutScheduledRuleCommand? captured = null;
        _sender
            .Send(
                Arg.Do<PutScheduledRuleCommand>(command => captured = command),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateScheduledRule(
            "daily-rule",
            "custom-bus",
            new ScheduledRuleUpdateRequest("rate(10 minutes)", "DISABLED", "updated"),
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("daily-rule");
        captured.ScheduleExpression.Should().Be("rate(10 minutes)");
        captured.State.Should().Be("DISABLED");
        captured.Description.Should().Be("updated");
        captured.EventBusName.Should().Be("custom-bus");
    }

    [Fact]
    public async Task UpdateScheduledRule_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<PutScheduledRuleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateScheduledRule(
            "daily-rule",
            null,
            new ScheduledRuleUpdateRequest("rate(10 minutes)", "ENABLED", null),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task DeleteScheduledRule_WhenCommandSucceeds_ReturnsNoContentAndForwardsRequest()
    {
        // Arrange
        DeleteScheduledRuleCommand? captured = null;
        _sender
            .Send(
                Arg.Do<DeleteScheduledRuleCommand>(command => captured = command),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteScheduledRule(
            "daily-rule", "custom-bus", TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("daily-rule");
        captured.EventBusName.Should().Be("custom-bus");
    }

    [Fact]
    public async Task DeleteScheduledRule_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<DeleteScheduledRuleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteScheduledRule(
            "daily-rule", null, TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task SetScheduledRuleState_WhenCommandSucceeds_ReturnsNoContentAndForwardsRequest()
    {
        // Arrange
        SetScheduledRuleStateCommand? captured = null;
        _sender
            .Send(
                Arg.Do<SetScheduledRuleStateCommand>(command => captured = command),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.SetScheduledRuleState(
            "daily-rule",
            "custom-bus",
            new ScheduledRuleStateRequest("DISABLED"),
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("daily-rule");
        captured.State.Should().Be("DISABLED");
        captured.EventBusName.Should().Be("custom-bus");
    }

    [Fact]
    public async Task SetScheduledRuleState_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<SetScheduledRuleStateCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.SetScheduledRuleState(
            "daily-rule",
            null,
            new ScheduledRuleStateRequest("ENABLED"),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task PutScheduledRuleTargets_WhenCommandSucceeds_ReturnsNoContentAndForwardsRequest()
    {
        // Arrange
        PutScheduledRuleTargetsCommand? captured = null;
        _sender
            .Send(
                Arg.Do<PutScheduledRuleTargetsCommand>(command => captured = command),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.PutScheduledRuleTargets(
            "daily-rule",
            "custom-bus",
            new ScheduledRuleTargetsPutRequest(
                [new ScheduledRuleTargetRequest("t1", "arn:aws:sqs:us-east-1:000000000000:queue", "role-arn", "{}")]),
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        captured.Should().NotBeNull();
        captured!.RuleName.Should().Be("daily-rule");
        captured.EventBusName.Should().Be("custom-bus");
        var target = captured.Targets.Should().ContainSingle().Subject;
        target.Id.Should().Be("t1");
        target.Arn.Should().Be("arn:aws:sqs:us-east-1:000000000000:queue");
        target.RoleArn.Should().Be("role-arn");
        target.Input.Should().Be("{}");
    }

    [Fact]
    public async Task PutScheduledRuleTargets_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<PutScheduledRuleTargetsCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.PutScheduledRuleTargets(
            "daily-rule",
            null,
            new ScheduledRuleTargetsPutRequest(
                [new ScheduledRuleTargetRequest("t1", "arn:aws:sqs:us-east-1:000000000000:queue", null, null)]),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }

    [Fact]
    public async Task RemoveScheduledRuleTargets_WhenCommandSucceeds_ReturnsNoContentAndForwardsRequest()
    {
        // Arrange
        RemoveScheduledRuleTargetsCommand? captured = null;
        _sender
            .Send(
                Arg.Do<RemoveScheduledRuleTargetsCommand>(command => captured = command),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        var sut = CreateSut();

        // Act
        var result = await sut.RemoveScheduledRuleTargets(
            "daily-rule",
            "custom-bus",
            new ScheduledRuleTargetsRemoveRequest(["t1", "t2"]),
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
        captured.Should().NotBeNull();
        captured!.RuleName.Should().Be("daily-rule");
        captured.EventBusName.Should().Be("custom-bus");
        captured.TargetIds.Should().BeEquivalentTo("t1", "t2");
    }

    [Fact]
    public async Task RemoveScheduledRuleTargets_WhenCommandFails_ReturnsErrorResult()
    {
        // Arrange
        _sender
            .Send(Arg.Any<RemoveScheduledRuleTargetsCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result>(new Error("boom")));
        var sut = CreateSut();

        // Act
        var result = await sut.RemoveScheduledRuleTargets(
            "daily-rule",
            null,
            new ScheduledRuleTargetsRemoveRequest(["t1"]),
            TestContext.Current.CancellationToken);

        // Assert
        var statusResult = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        statusResult.StatusCode.Should().BeGreaterThanOrEqualTo(400);
    }
}
