using AspNet.KickStarter.FunctionalResult;
using Foundation.Domain.EventBridge;

namespace Foundation.Application.EventBridge;

/// <summary>
/// Abstracts the EventBridge operations the application needs so the handlers stay free of any
/// direct AWS SDK dependency. The implementation flows every call through the resilient AWS gateway
/// and translates failures into a <see cref="Result"/> rather than throwing.
/// </summary>
public interface IEventBridgeClient
{
    /// <summary>
    /// List the rules available on the default event bus.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The rules, or an error when the backend cannot be reached.</returns>
    Task<Result<IReadOnlyList<EventBridgeRule>>> ListRulesAsync(
        CancellationToken cancellationToken);

    /// <summary>
    /// List the targets a single rule delivers matched events to.
    /// </summary>
    /// <param name="ruleName">The name of the rule whose targets to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The targets, or an error when the backend cannot be reached.</returns>
    Task<Result<IReadOnlyList<EventBridgeTarget>>> ListTargetsByRuleAsync(
        string ruleName, CancellationToken cancellationToken);

    /// <summary>
    /// Put a single custom event onto an event bus.
    /// </summary>
    /// <param name="source">The source that identifies the application emitting the event.</param>
    /// <param name="detailType">The detail type that describes the kind of event.</param>
    /// <param name="detail">The event detail as a JSON object string.</param>
    /// <param name="eventBusName">The target event bus name, or <c>null</c> to use the default bus.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The put outcome, or an error when the backend cannot be reached.</returns>
    Task<Result<EventBridgePutResult>> PutEventAsync(
        string source,
        string detailType,
        string detail,
        string? eventBusName,
        CancellationToken cancellationToken);
}
