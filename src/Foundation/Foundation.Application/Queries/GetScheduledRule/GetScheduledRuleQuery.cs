using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.EventBridge;

namespace Foundation.Application.Queries.GetScheduledRule;

/// <summary>
/// Get the full configuration of a single EventBridge scheduled rule.
/// </summary>
/// <param name="Name">The name of the rule to describe.</param>
/// <param name="EventBusName">The event bus the rule belongs to, or <c>null</c> to use the default bus.</param>
public record GetScheduledRuleQuery(string Name, string? EventBusName)
    : IQuery<GetScheduledRuleQueryResult>;

/// <summary>
/// The full configuration of a single EventBridge scheduled rule.
/// </summary>
/// <param name="Rule">The rule detail.</param>
public record GetScheduledRuleQueryResult(EventBridgeRuleDetail Rule);
