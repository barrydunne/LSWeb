using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Snapshot;

namespace Foundation.Application.Queries.ExportWorkspaceSnapshot;

/// <summary>
/// Query to export the current state of all workspace resources into a serialised snapshot.
/// </summary>
public sealed record ExportWorkspaceSnapshotQuery : IQuery<WorkspaceSnapshot>;
