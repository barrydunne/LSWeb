namespace Foundation.Api.Models;

/// <summary>
/// The aggregate outcome of a bulk action, including per-item results so that partial success is visible.
/// </summary>
/// <param name="OperationId">The identifier correlating this outcome with its lifecycle notifications.</param>
/// <param name="Action">The action that was requested.</param>
/// <param name="TotalCount">The total number of resources the action was applied to.</param>
/// <param name="SucceededCount">The number of resources the action succeeded for.</param>
/// <param name="FailedCount">The number of resources the action failed for.</param>
/// <param name="OverallState">The overall terminal state, either <c>Succeeded</c> or <c>Failed</c>.</param>
/// <param name="Items">The per-resource results in the order the resources were supplied.</param>
public sealed record BulkActionResponse(
    string OperationId,
    string Action,
    int TotalCount,
    int SucceededCount,
    int FailedCount,
    string OverallState,
    IReadOnlyList<BulkActionItemResponse> Items);

/// <summary>
/// The result of applying a bulk action to a single resource.
/// </summary>
/// <param name="ResourceId">The identifier of the resource the action targeted.</param>
/// <param name="Succeeded">Whether the action completed successfully for this resource.</param>
/// <param name="Error">A human-readable failure reason when the action did not succeed; otherwise <see langword="null"/>.</param>
public sealed record BulkActionItemResponse(string ResourceId, bool Succeeded, string? Error);
