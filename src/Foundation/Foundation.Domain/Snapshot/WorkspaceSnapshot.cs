namespace Foundation.Domain.Snapshot;

/// <summary>
/// Represents the state of all discoverable resources at a point in time, serialised for export or import.
/// </summary>
/// <param name="Id">A unique identifier for this snapshot.</param>
/// <param name="ExportedAt">The UTC timestamp when the snapshot was captured.</param>
/// <param name="Resources">All resources captured at the time of export, grouped by service.</param>
public record WorkspaceSnapshot(
    string Id,
    DateTime ExportedAt,
    IReadOnlyDictionary<string, IReadOnlyList<SnapshotResourceData>> Resources);

/// <summary>
/// Describes a single resource serialised within a workspace snapshot.
/// </summary>
/// <param name="ServiceKey">The catalogue key of the service, for example <c>lambda</c>.</param>
/// <param name="ResourceType">The resource type, for example <c>Function</c>.</param>
/// <param name="ResourceId">The ARN or unique identifier of the resource.</param>
/// <param name="ResourceName">The human-readable name of the resource.</param>
/// <param name="Data">The serialised resource data (JSON).</param>
public record SnapshotResourceData(
    string ServiceKey,
    string ResourceType,
    string ResourceId,
    string ResourceName,
    string Data);
