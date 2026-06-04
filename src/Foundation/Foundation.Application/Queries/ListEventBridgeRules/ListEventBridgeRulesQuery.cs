using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.EventBridge;

namespace Foundation.Application.Queries.ListEventBridgeRules;

/// <summary>
/// List the EventBridge rules available on the default event bus.
/// </summary>
public record ListEventBridgeRulesQuery : IQuery<ListEventBridgeRulesQueryResult>;

/// <summary>
/// The EventBridge rules available on the default event bus.
/// </summary>
/// <param name="Rules">The rules, ordered as returned by the backend.</param>
public record ListEventBridgeRulesQueryResult(IReadOnlyList<EventBridgeRule> Rules);
