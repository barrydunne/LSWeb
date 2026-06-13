using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.PutEventBridgeRule;

/// <summary>
/// Create or update an EventBridge event-pattern rule.
/// </summary>
/// <param name="Name">The rule name, unique within its event bus.</param>
/// <param name="EventPattern">The event pattern as a JSON object string that incoming events are matched against.</param>
/// <param name="State">The desired rule state, either <c>ENABLED</c> or <c>DISABLED</c>.</param>
/// <param name="Description">A human-readable description of the rule, or <see langword="null"/> to leave unset.</param>
/// <param name="EventBusName">The event bus the rule belongs to, or <see langword="null"/> to use the default bus.</param>
public record PutEventBridgeRuleCommand(
    string Name,
    string EventPattern,
    string State,
    string? Description,
    string? EventBusName) : ICommand;
