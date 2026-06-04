namespace Foundation.Api.Models;

/// <summary>
/// The EventBridge rules available on the default event bus.
/// </summary>
/// <param name="Rules">The rule summaries, ordered as returned by the backend.</param>
public sealed record RuleListResponse(
    IReadOnlyList<RuleSummaryResponse> Rules);

/// <summary>
/// A concise view of an EventBridge rule as it appears in a list.
/// </summary>
/// <param name="Name">The rule name, unique within its event bus.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the rule.</param>
/// <param name="EventBusName">The name of the event bus the rule belongs to.</param>
/// <param name="State">The rule state, either <c>ENABLED</c> or <c>DISABLED</c>.</param>
/// <param name="Description">An optional human-readable description of the rule.</param>
/// <param name="ScheduleExpression">The schedule expression for a scheduled rule, or <c>null</c> for an event-pattern rule.</param>
public sealed record RuleSummaryResponse(
    string Name,
    string Arn,
    string EventBusName,
    string State,
    string? Description,
    string? ScheduleExpression);

/// <summary>
/// The targets a single EventBridge rule delivers matched events to.
/// </summary>
/// <param name="Targets">The target summaries, ordered as returned by the backend.</param>
public sealed record TargetListResponse(
    IReadOnlyList<TargetSummaryResponse> Targets);

/// <summary>
/// A single target invoked when an EventBridge rule matches.
/// </summary>
/// <param name="Id">The target identifier, unique within the rule.</param>
/// <param name="Arn">The Amazon Resource Name of the resource the rule delivers events to.</param>
public sealed record TargetSummaryResponse(
    string Id,
    string Arn);

/// <summary>
/// A request to put a single custom event onto an EventBridge bus.
/// </summary>
/// <param name="Source">The source that identifies the application emitting the event.</param>
/// <param name="DetailType">The detail type that describes the kind of event.</param>
/// <param name="Detail">The event detail as a JSON object string.</param>
/// <param name="EventBusName">The target event bus name, or <c>null</c> to use the default bus.</param>
public sealed record PutEventRequest(
    string Source,
    string DetailType,
    string Detail,
    string? EventBusName);

/// <summary>
/// The outcome of putting a single custom event onto an EventBridge bus.
/// </summary>
/// <param name="Accepted">Whether EventBridge accepted the event.</param>
/// <param name="EventId">The identifier EventBridge assigned to the accepted event, or <c>null</c> when the entry failed.</param>
/// <param name="ErrorCode">The error code EventBridge returned for a rejected entry, or <c>null</c> when accepted.</param>
/// <param name="ErrorMessage">The error message EventBridge returned for a rejected entry, or <c>null</c> when accepted.</param>
public sealed record PutEventResponse(
    bool Accepted,
    string? EventId,
    string? ErrorCode,
    string? ErrorMessage);
