using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.RemoveScheduledRuleTargets;

/// <summary>
/// Remove one or more targets from a legacy EventBridge scheduled rule by their identifiers.
/// </summary>
/// <param name="RuleName">The name of the rule whose targets to remove.</param>
/// <param name="EventBusName">The event bus the rule belongs to, or <see langword="null"/> to use the default bus.</param>
/// <param name="TargetIds">The identifiers of the targets to remove.</param>
public record RemoveScheduledRuleTargetsCommand(
    string RuleName,
    string? EventBusName,
    IReadOnlyList<string> TargetIds) : ICommand;
