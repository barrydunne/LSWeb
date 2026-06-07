namespace Foundation.Api.Models;

/// <summary>
/// Response model for a successful snapshot export operation.
/// </summary>
/// <param name="SnapshotId">The identifier of the exported snapshot.</param>
/// <param name="ExportedAt">The UTC timestamp when the snapshot was captured.</param>
/// <param name="Services">The list of services that contributed resources to the snapshot.</param>
/// <param name="TotalResources">The total number of resources in the snapshot.</param>
public record SnapshotExportResponse(
    string SnapshotId,
    DateTime ExportedAt,
    IReadOnlyList<string> Services,
    int TotalResources);

/// <summary>
/// Response model for a snapshot import operation.
/// </summary>
/// <param name="OperationId">A unique identifier for this import operation.</param>
/// <param name="OperationType">The type of operation (Import).</param>
/// <param name="CompletedAt">The UTC timestamp when the operation completed.</param>
/// <param name="TotalResources">The total number of resources attempted.</param>
/// <param name="SuccessCount">The number of resources successfully imported.</param>
/// <param name="FailureCount">The number of resources that failed to import.</param>
/// <param name="Failures">Details of any failures that occurred.</param>
public record SnapshotImportResponse(
    string OperationId,
    string OperationType,
    DateTime CompletedAt,
    int TotalResources,
    int SuccessCount,
    int FailureCount,
    IReadOnlyList<SnapshotFailureResponse> Failures);

/// <summary>
/// Details of a failure encountered during snapshot import.
/// </summary>
/// <param name="Service">The service key where the failure occurred.</param>
/// <param name="ResourceId">The resource identifier that failed.</param>
/// <param name="Error">The error message describing the failure.</param>
public record SnapshotFailureResponse(string Service, string ResourceId, string Error);
