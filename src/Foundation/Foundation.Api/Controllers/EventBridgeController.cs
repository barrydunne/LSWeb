using AspNet.KickStarter.FunctionalResult.Extensions;
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
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Api.Controllers;

/// <summary>
/// Provides access to AWS EventBridge: listing the rules on the default event bus and the targets a
/// single rule delivers matched events to.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/services/eventbridge")]
public partial class EventBridgeController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventBridgeController"/> class.
    /// </summary>
    /// <param name="sender">The sender used to dispatch queries and commands.</param>
    /// <param name="logger">The logger.</param>
    public EventBridgeController(ISender sender, ILogger<EventBridgeController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Lists the EventBridge rules available on the backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the rule summaries.</returns>
    [HttpGet("rules")]
    [ProducesResponseType(typeof(RuleListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListRules(CancellationToken cancellationToken)
    {
        LogHandlingListRules();
        var result = await _sender.Send(new ListEventBridgeRulesQuery(), cancellationToken);
        LogListRulesHandled(result.IsSuccess);
        return result.Match(
            rules => Results.Ok(new RuleListResponse(
                rules.Rules
                    .Select(rule => new RuleSummaryResponse(
                        rule.Name,
                        rule.Arn,
                        rule.EventBusName,
                        rule.State,
                        rule.Description,
                        rule.ScheduleExpression))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the EventBridge scheduled rules (rules with a <c>rate(...)</c> or <c>cron(...)</c>
    /// schedule expression) available on the backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the scheduled rule summaries.</returns>
    [HttpGet("scheduled-rules")]
    [ProducesResponseType(typeof(ScheduledRuleListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListScheduledRules(CancellationToken cancellationToken)
    {
        LogHandlingListScheduledRules();
        var result = await _sender.Send(new ListScheduledRulesQuery(), cancellationToken);
        LogListScheduledRulesHandled(result.IsSuccess);
        return result.Match(
            rules => Results.Ok(new ScheduledRuleListResponse(
                rules.Rules
                    .Select(rule => new RuleSummaryResponse(
                        rule.Name,
                        rule.Arn,
                        rule.EventBusName,
                        rule.State,
                        rule.Description,
                        rule.ScheduleExpression))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Gets the full configuration of a single EventBridge scheduled rule.
    /// </summary>
    /// <param name="name">The name of the rule to describe.</param>
    /// <param name="bus">The event bus the rule belongs to, or <c>null</c> to use the default bus.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the rule detail.</returns>
    [HttpGet("scheduled-rules/{name}")]
    [ProducesResponseType(typeof(ScheduledRuleDetailResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetScheduledRule(
        string name, [FromQuery] string? bus, CancellationToken cancellationToken)
    {
        LogHandlingGetScheduledRule(name);
        var result = await _sender.Send(new GetScheduledRuleQuery(name, bus), cancellationToken);
        LogGetScheduledRuleHandled(result.IsSuccess);
        return result.Match(
            detail => Results.Ok(new ScheduledRuleDetailResponse(
                detail.Rule.Name,
                detail.Rule.Arn,
                detail.Rule.EventBusName,
                detail.Rule.State,
                detail.Rule.ScheduleExpression,
                detail.Rule.Description,
                detail.Rule.RoleArn,
                detail.Rule.ManagedBy)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the targets a single EventBridge rule delivers matched events to.
    /// </summary>
    /// <param name="rule">The name of the rule whose targets to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the target summaries.</returns>
    [HttpGet("targets")]
    [ProducesResponseType(typeof(TargetListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListTargets(
        [FromQuery] string rule, CancellationToken cancellationToken)
    {
        LogHandlingListTargets(rule);
        var result = await _sender.Send(new ListEventBridgeTargetsQuery(rule), cancellationToken);
        LogListTargetsHandled(result.IsSuccess);
        return result.Match(
            targets => Results.Ok(new TargetListResponse(
                targets.Targets
                    .Select(target => new TargetSummaryResponse(
                        target.Id,
                        target.Arn))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Puts a single custom event onto an EventBridge bus.
    /// </summary>
    /// <param name="request">The event source, detail type, JSON detail and optional bus name.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the put outcome.</returns>
    [HttpPost("events")]
    [ProducesResponseType(typeof(PutEventResponse), StatusCodes.Status200OK)]
    public async Task<IResult> PutEvent(
        [FromBody] PutEventRequest request, CancellationToken cancellationToken)
    {
        LogHandlingPutEvent(request.Source);
        var result = await _sender.Send(
            new PutEventBridgeEventCommand(
                request.Source,
                request.DetailType,
                request.Detail,
                request.EventBusName),
            cancellationToken);
        LogPutEventHandled(result.IsSuccess);
        return result.Match(
            outcome => Results.Ok(new PutEventResponse(
                outcome.Accepted,
                outcome.EventId,
                outcome.ErrorCode,
                outcome.ErrorMessage)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates a legacy EventBridge scheduled rule.
    /// </summary>
    /// <param name="request">The rule name, schedule expression, state and optional description and bus.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result locating the created rule.</returns>
    [HttpPost("scheduled-rules")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IResult> CreateScheduledRule(
        [FromBody] ScheduledRulePutRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreateScheduledRule(request.Name);
        var result = await _sender.Send(
            new PutScheduledRuleCommand(
                request.Name,
                request.ScheduleExpression,
                request.State,
                request.Description,
                request.EventBusName),
            cancellationToken);
        LogCreateScheduledRuleHandled(result.IsSuccess);
        return result.Match(
            () => Results.Created(
                $"/api/services/eventbridge/scheduled-rules/{Uri.EscapeDataString(request.Name)}", null),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Updates an existing legacy EventBridge scheduled rule.
    /// </summary>
    /// <param name="name">The name of the rule to update.</param>
    /// <param name="bus">The event bus the rule belongs to, or <c>null</c> to use the default bus.</param>
    /// <param name="request">The schedule expression, state and optional description.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("scheduled-rules/{name}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> UpdateScheduledRule(
        string name, [FromQuery] string? bus,
        [FromBody] ScheduledRuleUpdateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingUpdateScheduledRule(name);
        var result = await _sender.Send(
            new PutScheduledRuleCommand(
                name,
                request.ScheduleExpression,
                request.State,
                request.Description,
                bus),
            cancellationToken);
        LogUpdateScheduledRuleHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes a legacy EventBridge scheduled rule.
    /// </summary>
    /// <param name="name">The name of the rule to delete.</param>
    /// <param name="bus">The event bus the rule belongs to, or <c>null</c> to use the default bus.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("scheduled-rules/{name}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteScheduledRule(
        string name, [FromQuery] string? bus, CancellationToken cancellationToken)
    {
        LogHandlingDeleteScheduledRule(name);
        var result = await _sender.Send(new DeleteScheduledRuleCommand(name, bus), cancellationToken);
        LogDeleteScheduledRuleHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Enables or disables a legacy EventBridge scheduled rule.
    /// </summary>
    /// <param name="name">The name of the rule whose state to change.</param>
    /// <param name="bus">The event bus the rule belongs to, or <c>null</c> to use the default bus.</param>
    /// <param name="request">The desired rule state.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPost("scheduled-rules/{name}/state")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> SetScheduledRuleState(
        string name, [FromQuery] string? bus,
        [FromBody] ScheduledRuleStateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingSetScheduledRuleState(name, request.State);
        var result = await _sender.Send(
            new SetScheduledRuleStateCommand(name, request.State, bus), cancellationToken);
        LogSetScheduledRuleStateHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Adds or replaces targets on a legacy EventBridge scheduled rule.
    /// </summary>
    /// <param name="name">The name of the rule whose targets to write.</param>
    /// <param name="bus">The event bus the rule belongs to, or <c>null</c> to use the default bus.</param>
    /// <param name="request">The targets to add or replace.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("scheduled-rules/{name}/targets")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> PutScheduledRuleTargets(
        string name, [FromQuery] string? bus,
        [FromBody] ScheduledRuleTargetsPutRequest request, CancellationToken cancellationToken)
    {
        LogHandlingPutScheduledRuleTargets(name, request.Targets.Count);
        var result = await _sender.Send(
            new PutScheduledRuleTargetsCommand(
                name,
                bus,
                request.Targets
                    .Select(target => new EventBridgeTargetSpecification(
                        target.Id,
                        target.Arn,
                        target.RoleArn,
                        target.Input))
                    .ToList()),
            cancellationToken);
        LogPutScheduledRuleTargetsHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Removes targets from a legacy EventBridge scheduled rule.
    /// </summary>
    /// <param name="name">The name of the rule whose targets to remove.</param>
    /// <param name="bus">The event bus the rule belongs to, or <c>null</c> to use the default bus.</param>
    /// <param name="request">The identifiers of the targets to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("scheduled-rules/{name}/targets")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> RemoveScheduledRuleTargets(
        string name, [FromQuery] string? bus,
        [FromBody] ScheduledRuleTargetsRemoveRequest request, CancellationToken cancellationToken)
    {
        LogHandlingRemoveScheduledRuleTargets(name, request.Ids.Count);
        var result = await _sender.Send(
            new RemoveScheduledRuleTargetsCommand(name, bus, request.Ids), cancellationToken);
        LogRemoveScheduledRuleTargetsHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    [LoggerMessage(LogLevel.Trace, "Handling EventBridge rule list request.")]
    private partial void LogHandlingListRules();

    [LoggerMessage(LogLevel.Trace, "EventBridge rule list request handled. Success: {Success}")]
    private partial void LogListRulesHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling EventBridge scheduled rule list request.")]
    private partial void LogHandlingListScheduledRules();

    [LoggerMessage(LogLevel.Trace, "EventBridge scheduled rule list request handled. Success: {Success}")]
    private partial void LogListScheduledRulesHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling EventBridge scheduled rule detail request for {Rule}.")]
    private partial void LogHandlingGetScheduledRule(string rule);

    [LoggerMessage(LogLevel.Trace, "EventBridge scheduled rule detail request handled. Success: {Success}")]
    private partial void LogGetScheduledRuleHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling EventBridge target list request for {Rule}.")]
    private partial void LogHandlingListTargets(string rule);

    [LoggerMessage(LogLevel.Trace, "EventBridge target list request handled. Success: {Success}")]
    private partial void LogListTargetsHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling EventBridge put event request from {Source}.")]
    private partial void LogHandlingPutEvent(string source);

    [LoggerMessage(LogLevel.Trace, "EventBridge put event request handled. Success: {Success}")]
    private partial void LogPutEventHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling EventBridge create scheduled rule request for {Name}.")]
    private partial void LogHandlingCreateScheduledRule(string name);

    [LoggerMessage(LogLevel.Trace, "EventBridge create scheduled rule request handled. Success: {Success}")]
    private partial void LogCreateScheduledRuleHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling EventBridge update scheduled rule request for {Name}.")]
    private partial void LogHandlingUpdateScheduledRule(string name);

    [LoggerMessage(LogLevel.Trace, "EventBridge update scheduled rule request handled. Success: {Success}")]
    private partial void LogUpdateScheduledRuleHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling EventBridge delete scheduled rule request for {Name}.")]
    private partial void LogHandlingDeleteScheduledRule(string name);

    [LoggerMessage(LogLevel.Trace, "EventBridge delete scheduled rule request handled. Success: {Success}")]
    private partial void LogDeleteScheduledRuleHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling EventBridge set scheduled rule {Name} state to {State}.")]
    private partial void LogHandlingSetScheduledRuleState(string name, string state);

    [LoggerMessage(LogLevel.Trace, "EventBridge set scheduled rule state request handled. Success: {Success}")]
    private partial void LogSetScheduledRuleStateHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling EventBridge put {Count} target(s) on scheduled rule {Name}.")]
    private partial void LogHandlingPutScheduledRuleTargets(string name, int count);

    [LoggerMessage(LogLevel.Trace, "EventBridge put scheduled rule targets request handled. Success: {Success}")]
    private partial void LogPutScheduledRuleTargetsHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling EventBridge remove {Count} target(s) from scheduled rule {Name}.")]
    private partial void LogHandlingRemoveScheduledRuleTargets(string name, int count);

    [LoggerMessage(LogLevel.Trace, "EventBridge remove scheduled rule targets request handled. Success: {Success}")]
    private partial void LogRemoveScheduledRuleTargetsHandled(bool success);
}
