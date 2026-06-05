using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.PutScheduledRule;

/// <summary>
/// Create or update a legacy EventBridge scheduled rule from the supplied configuration.
/// </summary>
/// <param name="Name">The rule name, unique within its event bus.</param>
/// <param name="ScheduleExpression">The <c>rate(...)</c> or <c>cron(...)</c> expression that controls when the rule fires.</param>
/// <param name="State">The desired rule state, either <c>ENABLED</c> or <c>DISABLED</c>.</param>
/// <param name="Description">A human-readable description of the rule, or <see langword="null"/> to leave unset.</param>
/// <param name="EventBusName">The event bus the rule belongs to, or <see langword="null"/> to use the default bus.</param>
public record PutScheduledRuleCommand(
    string Name,
    string ScheduleExpression,
    string State,
    string? Description,
    string? EventBusName) : ICommand;
