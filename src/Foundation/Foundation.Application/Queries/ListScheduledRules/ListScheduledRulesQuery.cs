using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.EventBridge;

namespace Foundation.Application.Queries.ListScheduledRules;

/// <summary>
/// List the EventBridge scheduled rules (rules with a <c>rate(...)</c> or <c>cron(...)</c> schedule
/// expression) available on the default event bus.
/// </summary>
public record ListScheduledRulesQuery : IQuery<ListScheduledRulesQueryResult>;

/// <summary>
/// The EventBridge scheduled rules available on the default event bus.
/// </summary>
/// <param name="Rules">The scheduled rules, ordered as returned by the backend.</param>
public record ListScheduledRulesQueryResult(IReadOnlyList<EventBridgeRule> Rules);
