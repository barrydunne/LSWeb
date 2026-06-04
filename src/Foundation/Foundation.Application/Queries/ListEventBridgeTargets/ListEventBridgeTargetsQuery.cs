using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.EventBridge;

namespace Foundation.Application.Queries.ListEventBridgeTargets;

/// <summary>
/// List the targets a single EventBridge rule delivers matched events to.
/// </summary>
/// <param name="RuleName">The name of the rule whose targets to list.</param>
public record ListEventBridgeTargetsQuery(string RuleName)
    : IQuery<ListEventBridgeTargetsQueryResult>;

/// <summary>
/// The targets a single EventBridge rule delivers matched events to.
/// </summary>
/// <param name="Targets">The targets, ordered as returned by the backend.</param>
public record ListEventBridgeTargetsQueryResult(IReadOnlyList<EventBridgeTarget> Targets);
