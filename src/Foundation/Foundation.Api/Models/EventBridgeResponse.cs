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
/// The full configuration of a single EventBridge rule, including its event pattern.
/// </summary>
/// <param name="Name">The rule name, unique within its event bus.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the rule.</param>
/// <param name="EventBusName">The name of the event bus the rule belongs to.</param>
/// <param name="State">The rule state, either <c>ENABLED</c> or <c>DISABLED</c>.</param>
/// <param name="ScheduleExpression">The schedule expression for a scheduled rule, or <c>null</c> for an event-pattern rule.</param>
/// <param name="Description">An optional human-readable description of the rule.</param>
/// <param name="RoleArn">The IAM role the rule assumes when invoking targets, or <c>null</c> when none is configured.</param>
/// <param name="ManagedBy">The principal that created the rule on the caller's behalf, or <c>null</c> for an unmanaged rule.</param>
/// <param name="EventPattern">The event pattern JSON for an event-pattern rule, or <c>null</c> for a scheduled rule.</param>
public sealed record RuleDetailResponse(
    string Name,
    string Arn,
    string EventBusName,
    string State,
    string? ScheduleExpression,
    string? Description,
    string? RoleArn,
    string? ManagedBy,
    string? EventPattern);

/// <summary>
/// A request to create or replace an EventBridge event-pattern rule.
/// </summary>
/// <param name="Name">The rule name, unique within its event bus.</param>
/// <param name="EventPattern">The event pattern as a JSON object string that incoming events are matched against.</param>
/// <param name="State">The desired rule state, either <c>ENABLED</c> or <c>DISABLED</c>.</param>
/// <param name="Description">An optional human-readable description of the rule.</param>
/// <param name="EventBusName">The target event bus name, or <c>null</c> to use the default bus.</param>
public sealed record RulePutRequest(
    string Name,
    string EventPattern,
    string State,
    string? Description,
    string? EventBusName);

/// <summary>
/// A single target to add or replace on an EventBridge rule.
/// </summary>
/// <param name="Id">The target identifier, unique within the rule.</param>
/// <param name="Arn">The Amazon Resource Name of the resource the rule delivers events to.</param>
/// <param name="RoleArn">The IAM role the rule assumes when invoking the target, or <c>null</c> when none is required.</param>
/// <param name="Input">A constant JSON text passed to the target, or <c>null</c> to pass the matched event.</param>
public sealed record RuleTargetRequest(
    string Id,
    string Arn,
    string? RoleArn,
    string? Input);

/// <summary>
/// A request to add or replace targets on an EventBridge rule.
/// </summary>
/// <param name="Targets">The targets to add or replace, matched by their identifiers.</param>
public sealed record RuleTargetsPutRequest(
    IReadOnlyList<RuleTargetRequest> Targets);

/// <summary>
/// A request to remove targets from an EventBridge rule.
/// </summary>
/// <param name="Ids">The identifiers of the targets to remove.</param>
public sealed record RuleTargetsRemoveRequest(
    IReadOnlyList<string> Ids);

/// <summary>
/// The EventBridge event buses available on the backend.
/// </summary>
/// <param name="Buses">The event bus summaries, ordered as returned by the backend.</param>
public sealed record EventBusListResponse(
    IReadOnlyList<EventBusSummaryResponse> Buses);

/// <summary>
/// A concise view of an EventBridge event bus.
/// </summary>
/// <param name="Name">The event bus name, unique within the account and region.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the event bus.</param>
public sealed record EventBusSummaryResponse(
    string Name,
    string Arn);

/// <summary>
/// A request to create a custom EventBridge event bus.
/// </summary>
/// <param name="Name">The name of the event bus to create.</param>
public sealed record EventBusCreateRequest(
    string Name);

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

/// <summary>
/// The EventBridge scheduled rules available on the default event bus.
/// </summary>
/// <param name="Rules">The scheduled rule summaries, ordered as returned by the backend.</param>
public sealed record ScheduledRuleListResponse(
    IReadOnlyList<RuleSummaryResponse> Rules);

/// <summary>
/// The full configuration of a single EventBridge scheduled rule.
/// </summary>
/// <param name="Name">The rule name, unique within its event bus.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the rule.</param>
/// <param name="EventBusName">The name of the event bus the rule belongs to.</param>
/// <param name="State">The rule state, either <c>ENABLED</c> or <c>DISABLED</c>.</param>
/// <param name="ScheduleExpression">The schedule expression for the rule, or <c>null</c> when none is configured.</param>
/// <param name="Description">An optional human-readable description of the rule.</param>
/// <param name="RoleArn">The IAM role the rule assumes when invoking targets, or <c>null</c> when none is configured.</param>
/// <param name="ManagedBy">The principal that created the rule on the caller's behalf, or <c>null</c> for an unmanaged rule.</param>
public sealed record ScheduledRuleDetailResponse(
    string Name,
    string Arn,
    string EventBusName,
    string State,
    string? ScheduleExpression,
    string? Description,
    string? RoleArn,
    string? ManagedBy);

/// <summary>
/// A request to create or replace a legacy EventBridge scheduled rule.
/// </summary>
/// <param name="Name">The rule name, unique within its event bus.</param>
/// <param name="ScheduleExpression">The schedule expression, starting with <c>rate(</c> or <c>cron(</c>.</param>
/// <param name="State">The desired rule state, either <c>ENABLED</c> or <c>DISABLED</c>.</param>
/// <param name="Description">An optional human-readable description of the rule.</param>
/// <param name="EventBusName">The target event bus name, or <c>null</c> to use the default bus.</param>
public sealed record ScheduledRulePutRequest(
    string Name,
    string ScheduleExpression,
    string State,
    string? Description,
    string? EventBusName);

/// <summary>
/// A request to update an existing legacy EventBridge scheduled rule.
/// </summary>
/// <param name="ScheduleExpression">The schedule expression, starting with <c>rate(</c> or <c>cron(</c>.</param>
/// <param name="State">The desired rule state, either <c>ENABLED</c> or <c>DISABLED</c>.</param>
/// <param name="Description">An optional human-readable description of the rule.</param>
public sealed record ScheduledRuleUpdateRequest(
    string ScheduleExpression,
    string State,
    string? Description);

/// <summary>
/// A request to enable or disable a legacy EventBridge scheduled rule.
/// </summary>
/// <param name="State">The desired rule state, either <c>ENABLED</c> or <c>DISABLED</c>.</param>
public sealed record ScheduledRuleStateRequest(
    string State);

/// <summary>
/// A single target to add or replace on a legacy EventBridge scheduled rule.
/// </summary>
/// <param name="Id">The target identifier, unique within the rule.</param>
/// <param name="Arn">The Amazon Resource Name of the resource the rule delivers events to.</param>
/// <param name="RoleArn">The IAM role the rule assumes when invoking the target, or <c>null</c> when none is required.</param>
/// <param name="Input">A constant JSON text passed to the target, or <c>null</c> to pass the matched event.</param>
public sealed record ScheduledRuleTargetRequest(
    string Id,
    string Arn,
    string? RoleArn,
    string? Input);

/// <summary>
/// A request to add or replace targets on a legacy EventBridge scheduled rule.
/// </summary>
/// <param name="Targets">The targets to add or replace, matched by their identifiers.</param>
public sealed record ScheduledRuleTargetsPutRequest(
    IReadOnlyList<ScheduledRuleTargetRequest> Targets);

/// <summary>
/// A request to remove targets from a legacy EventBridge scheduled rule.
/// </summary>
/// <param name="Ids">The identifiers of the targets to remove.</param>
public sealed record ScheduledRuleTargetsRemoveRequest(
    IReadOnlyList<string> Ids);
