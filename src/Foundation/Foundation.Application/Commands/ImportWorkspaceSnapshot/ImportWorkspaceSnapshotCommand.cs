using AspNet.KickStarter.CQRS.Abstractions.Commands;
using Foundation.Domain.Snapshot;

namespace Foundation.Application.Commands.ImportWorkspaceSnapshot;

/// <summary>
/// Command to import a previously exported workspace snapshot, recreating its resources.
/// </summary>
/// <param name="Snapshot">The snapshot data to import.</param>
public sealed record ImportWorkspaceSnapshotCommand(WorkspaceSnapshot Snapshot) : ICommand<SnapshotOutcome>;
