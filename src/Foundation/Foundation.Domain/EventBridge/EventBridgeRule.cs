namespace Foundation.Domain.EventBridge;

/// <summary>
/// A concise view of an EventBridge rule as it appears in a list.
/// </summary>
/// <param name="Name">The rule name, unique within its event bus.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the rule.</param>
/// <param name="EventBusName">The name of the event bus the rule belongs to.</param>
/// <param name="State">The rule state, either <c>ENABLED</c> or <c>DISABLED</c>.</param>
/// <param name="Description">An optional human-readable description of the rule.</param>
/// <param name="ScheduleExpression">The schedule expression for a scheduled rule, or <c>null</c> for an event-pattern rule.</param>
public sealed record EventBridgeRule(
    string Name,
    string Arn,
    string EventBusName,
    string State,
    string? Description,
    string? ScheduleExpression);
