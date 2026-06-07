namespace Foundation.Domain.Snapshot;

/// <summary>
/// The outcome of an export or import operation on a workspace snapshot.
/// </summary>
/// <param name="OperationId">A unique identifier for this operation.</param>
/// <param name="OperationType">The type of operation (Export or Import).</param>
/// <param name="CompletedAt">The UTC timestamp when the operation completed.</param>
/// <param name="ResourceCount">The total number of resources affected.</param>
/// <param name="SuccessCount">The number of resources successfully processed.</param>
/// <param name="FailureCount">The number of resources that failed.</param>
/// <param name="Failures">Details of any failures that occurred.</param>
public record SnapshotOutcome(
    string OperationId,
    string OperationType,
    DateTime CompletedAt,
    int ResourceCount,
    int SuccessCount,
    int FailureCount,
    IReadOnlyList<SnapshotFailureDetail> Failures);

/// <summary>
/// Details of a failure encountered during snapshot export or import.
/// </summary>
/// <param name="Service">The service key where the failure occurred.</param>
/// <param name="ResourceId">The resource identifier.</param>
/// <param name="Error">The error message.</param>
public record SnapshotFailureDetail(string Service, string ResourceId, string Error);
