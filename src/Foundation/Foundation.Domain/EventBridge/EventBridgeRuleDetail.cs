namespace Foundation.Domain.EventBridge;

/// <summary>
/// The full configuration of a single EventBridge rule, as returned by a describe call.
/// </summary>
/// <param name="Name">The rule name, unique within its event bus.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the rule.</param>
/// <param name="EventBusName">The name of the event bus the rule belongs to.</param>
/// <param name="State">The rule state, either <c>ENABLED</c> or <c>DISABLED</c>.</param>
/// <param name="ScheduleExpression">The schedule expression for a scheduled rule, or <c>null</c> for an event-pattern rule.</param>
/// <param name="Description">An optional human-readable description of the rule.</param>
/// <param name="RoleArn">The IAM role the rule assumes when invoking targets, or <c>null</c> when none is configured.</param>
/// <param name="ManagedBy">The principal that created the rule on the caller's behalf, or <c>null</c> for an unmanaged rule.</param>
public sealed record EventBridgeRuleDetail(
    string Name,
    string Arn,
    string EventBusName,
    string State,
    string? ScheduleExpression,
    string? Description,
    string? RoleArn,
    string? ManagedBy);
