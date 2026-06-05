using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.SetScheduledRuleState;

/// <summary>
/// Enable or disable a legacy EventBridge scheduled rule.
/// </summary>
/// <param name="Name">The name of the rule whose state to change.</param>
/// <param name="State">The desired rule state, either <c>ENABLED</c> or <c>DISABLED</c>.</param>
/// <param name="EventBusName">The event bus the rule belongs to, or <see langword="null"/> to use the default bus.</param>
public record SetScheduledRuleStateCommand(
    string Name,
    string State,
    string? EventBusName) : ICommand;
