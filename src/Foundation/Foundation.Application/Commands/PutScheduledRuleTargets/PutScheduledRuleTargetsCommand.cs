using AspNet.KickStarter.CQRS.Abstractions.Commands;
using Foundation.Domain.EventBridge;

namespace Foundation.Application.Commands.PutScheduledRuleTargets;

/// <summary>
/// Add or replace one or more targets on a legacy EventBridge scheduled rule.
/// </summary>
/// <param name="RuleName">The name of the rule whose targets to write.</param>
/// <param name="EventBusName">The event bus the rule belongs to, or <see langword="null"/> to use the default bus.</param>
/// <param name="Targets">The targets to add or replace, matched by their identifiers.</param>
public record PutScheduledRuleTargetsCommand(
    string RuleName,
    string? EventBusName,
    IReadOnlyList<EventBridgeTargetSpecification> Targets) : ICommand;
