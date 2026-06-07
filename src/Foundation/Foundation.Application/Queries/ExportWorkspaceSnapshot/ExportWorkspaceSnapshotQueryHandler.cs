using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Snapshot;
using Foundation.Domain.Snapshot;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ExportWorkspaceSnapshot;

/// <summary>
/// Handles exporting the current workspace state by iterating through all discoverable resources and
/// serialising them into a snapshot document.
/// </summary>
internal sealed partial class ExportWorkspaceSnapshotQueryHandler : IQueryHandler<ExportWorkspaceSnapshotQuery, WorkspaceSnapshot>
{
    private readonly IWorkspaceSnapshotExporter _exporter;
    private readonly ILogger<ExportWorkspaceSnapshotQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExportWorkspaceSnapshotQueryHandler"/> class.
    /// </summary>
    /// <param name="exporter">The snapshot exporter service.</param>
    /// <param name="logger">The logger.</param>
    public ExportWorkspaceSnapshotQueryHandler(
        IWorkspaceSnapshotExporter exporter,
        ILogger<ExportWorkspaceSnapshotQueryHandler> logger)
    {
        _exporter = exporter;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<WorkspaceSnapshot>> Handle(ExportWorkspaceSnapshotQuery request, CancellationToken cancellationToken)
    {
        LogHandling();

        try
        {
            var snapshot = await _exporter.ExportAsync(cancellationToken);
            var resourceCount = snapshot.Resources.Values.Sum(_ => _.Count);
            LogExported(resourceCount);
            return snapshot;
        }
        catch (Exception ex)
        {
            LogFailed(ex.Message);
            return new Error($"Failed to export workspace: {ex.Message}");
        }
    }

    [LoggerMessage(LogLevel.Information, "Exporting workspace snapshot")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Information, "Workspace snapshot exported with {ResourceCount} resources")]
    private partial void LogExported(int resourceCount);

    [LoggerMessage(LogLevel.Error, "Failed to export workspace snapshot: {Error}")]
    private partial void LogFailed(string error);
}
