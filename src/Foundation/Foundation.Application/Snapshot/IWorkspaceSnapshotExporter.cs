using Foundation.Domain.Snapshot;

namespace Foundation.Application.Snapshot;

/// <summary>
/// Service for exporting the current workspace state into a serialised snapshot.
/// </summary>
public interface IWorkspaceSnapshotExporter
{
    /// <summary>
    /// Exports the current state of all discoverable workspace resources into a snapshot.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A snapshot containing all resources.</returns>
    Task<WorkspaceSnapshot> ExportAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Service for importing a previously exported workspace snapshot, recreating its resources.
/// </summary>
public interface IWorkspaceSnapshotImporter
{
    /// <summary>
    /// Imports a workspace snapshot, recreating each of its resources and returning per-resource results.
    /// </summary>
    /// <param name="snapshot">The snapshot to import.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An outcome describing the success/failure of each resource import.</returns>
    Task<SnapshotOutcome> ImportAsync(WorkspaceSnapshot snapshot, CancellationToken cancellationToken);
}
