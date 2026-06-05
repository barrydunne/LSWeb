using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteScheduledRule;

/// <summary>
/// Delete a legacy EventBridge scheduled rule. The rule must have no remaining targets.
/// </summary>
/// <param name="Name">The name of the rule to delete.</param>
/// <param name="EventBusName">The event bus the rule belongs to, or <see langword="null"/> to use the default bus.</param>
public record DeleteScheduledRuleCommand(
    string Name,
    string? EventBusName) : ICommand;
