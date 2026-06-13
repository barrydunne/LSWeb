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
    /// Describe the full configuration of a single rule.
    /// </summary>
    /// <param name="ruleName">The name of the rule to describe.</param>
    /// <param name="eventBusName">The event bus the rule belongs to, or <c>null</c> to use the default bus.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The rule detail, or an error when the backend cannot be reached.</returns>
    Task<Result<EventBridgeRuleDetail>> DescribeRuleAsync(
        string ruleName, string? eventBusName, CancellationToken cancellationToken);

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

    /// <summary>
    /// Create or update a scheduled rule from the supplied specification.
    /// </summary>
    /// <param name="specification">The rule configuration to apply.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error when the rule cannot be written.</returns>
    Task<Result> PutRuleAsync(
        EventBridgeRuleSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Create or update an event-pattern rule from the supplied specification.
    /// </summary>
    /// <param name="specification">The event-pattern rule configuration to apply.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error when the rule cannot be written.</returns>
    Task<Result> PutEventPatternRuleAsync(
        EventBridgeRulePatternSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Delete a rule by its name. The rule must have no remaining targets before it can be deleted.
    /// </summary>
    /// <param name="ruleName">The name of the rule to delete.</param>
    /// <param name="eventBusName">The event bus the rule belongs to, or <c>null</c> to use the default bus.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error when the rule cannot be deleted.</returns>
    Task<Result> DeleteRuleAsync(
        string ruleName, string? eventBusName, CancellationToken cancellationToken);

    /// <summary>
    /// Enable a rule so that it fires on its schedule.
    /// </summary>
    /// <param name="ruleName">The name of the rule to enable.</param>
    /// <param name="eventBusName">The event bus the rule belongs to, or <c>null</c> to use the default bus.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error when the rule cannot be enabled.</returns>
    Task<Result> EnableRuleAsync(
        string ruleName, string? eventBusName, CancellationToken cancellationToken);

    /// <summary>
    /// Disable a rule so that it stops firing on its schedule.
    /// </summary>
    /// <param name="ruleName">The name of the rule to disable.</param>
    /// <param name="eventBusName">The event bus the rule belongs to, or <c>null</c> to use the default bus.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error when the rule cannot be disabled.</returns>
    Task<Result> DisableRuleAsync(
        string ruleName, string? eventBusName, CancellationToken cancellationToken);

    /// <summary>
    /// Add or replace one or more targets on a rule.
    /// </summary>
    /// <param name="ruleName">The name of the rule whose targets to write.</param>
    /// <param name="eventBusName">The event bus the rule belongs to, or <c>null</c> to use the default bus.</param>
    /// <param name="targets">The targets to add or replace, matched by their identifiers.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error when the targets cannot be written.</returns>
    Task<Result> PutTargetsAsync(
        string ruleName,
        string? eventBusName,
        IReadOnlyList<EventBridgeTargetSpecification> targets,
        CancellationToken cancellationToken);

    /// <summary>
    /// Remove one or more targets from a rule by their identifiers.
    /// </summary>
    /// <param name="ruleName">The name of the rule whose targets to remove.</param>
    /// <param name="eventBusName">The event bus the rule belongs to, or <c>null</c> to use the default bus.</param>
    /// <param name="targetIds">The identifiers of the targets to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error when the targets cannot be removed.</returns>
    Task<Result> RemoveTargetsAsync(
        string ruleName,
        string? eventBusName,
        IReadOnlyList<string> targetIds,
        CancellationToken cancellationToken);

    /// <summary>
    /// List the event buses available on the backend, including the default bus and any custom buses.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The event buses, or an error when the backend cannot be reached.</returns>
    Task<Result<IReadOnlyList<EventBridgeEventBus>>> ListEventBusesAsync(
        CancellationToken cancellationToken);

    /// <summary>
    /// Create a custom event bus with the supplied name.
    /// </summary>
    /// <param name="name">The name of the event bus to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error when the bus cannot be created.</returns>
    Task<Result> CreateEventBusAsync(string name, CancellationToken cancellationToken);

    /// <summary>
    /// Delete a custom event bus by name. The default bus cannot be deleted.
    /// </summary>
    /// <param name="name">The name of the event bus to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error when the bus cannot be deleted.</returns>
    Task<Result> DeleteEventBusAsync(string name, CancellationToken cancellationToken);
}
