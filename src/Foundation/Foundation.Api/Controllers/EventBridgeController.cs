using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Commands.PutEventBridgeEvent;
using Foundation.Application.Queries.ListEventBridgeRules;
using Foundation.Application.Queries.ListEventBridgeTargets;
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

    [LoggerMessage(LogLevel.Trace, "Handling EventBridge rule list request.")]
    private partial void LogHandlingListRules();

    [LoggerMessage(LogLevel.Trace, "EventBridge rule list request handled. Success: {Success}")]
    private partial void LogListRulesHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling EventBridge target list request for {Rule}.")]
    private partial void LogHandlingListTargets(string rule);

    [LoggerMessage(LogLevel.Trace, "EventBridge target list request handled. Success: {Success}")]
    private partial void LogListTargetsHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling EventBridge put event request from {Source}.")]
    private partial void LogHandlingPutEvent(string source);

    [LoggerMessage(LogLevel.Trace, "EventBridge put event request handled. Success: {Success}")]
    private partial void LogPutEventHandled(bool success);
}
