using Foundation.Domain.Streaming;

namespace Foundation.Domain.Bulk;

/// <summary>
/// The aggregate outcome of executing a bulk action across a set of resources, carrying the
/// per-item results so that partial success can be reported to the caller.
/// </summary>
/// <param name="OperationId">The identifier correlating this outcome with its lifecycle notifications.</param>
/// <param name="Action">The action that was requested, for example <c>delete</c>.</param>
/// <param name="Items">The per-resource results in the order the resources were supplied.</param>
public record BulkActionOutcome(string OperationId, string Action, IReadOnlyList<BulkActionItemResult> Items)
{
    /// <summary>
    /// Gets the total number of resources the action was applied to.
    /// </summary>
    public int TotalCount => Items.Count;

    /// <summary>
    /// Gets the number of resources the action succeeded for.
    /// </summary>
    public int SucceededCount => Items.Count(item => item.Succeeded);

    /// <summary>
    /// Gets the number of resources the action failed for.
    /// </summary>
    public int FailedCount => Items.Count(item => !item.Succeeded);

    /// <summary>
    /// Gets the overall terminal state: <see cref="OperationState.Succeeded"/> when every item
    /// succeeded, otherwise <see cref="OperationState.Failed"/>.
    /// </summary>
    public OperationState OverallState => FailedCount == 0 ? OperationState.Succeeded : OperationState.Failed;
}
