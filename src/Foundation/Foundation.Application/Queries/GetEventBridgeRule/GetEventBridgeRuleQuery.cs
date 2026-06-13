using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.EventBridge;

namespace Foundation.Application.Queries.GetEventBridgeRule;

/// <summary>
/// Get the full configuration of a single EventBridge rule, including its event pattern.
/// </summary>
/// <param name="Name">The name of the rule to describe.</param>
/// <param name="EventBusName">The event bus the rule belongs to, or <c>null</c> to use the default bus.</param>
public record GetEventBridgeRuleQuery(string Name, string? EventBusName)
    : IQuery<GetEventBridgeRuleQueryResult>;

/// <summary>
/// The full configuration of a single EventBridge rule.
/// </summary>
/// <param name="Rule">The rule detail.</param>
public record GetEventBridgeRuleQueryResult(EventBridgeRuleDetail Rule);
