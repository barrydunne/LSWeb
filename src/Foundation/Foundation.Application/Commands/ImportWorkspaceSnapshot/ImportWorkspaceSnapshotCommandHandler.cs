using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Snapshot;
using Foundation.Domain.Snapshot;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.ImportWorkspaceSnapshot;

/// <summary>
/// Handles importing a workspace snapshot by deserialising and recreating each resource through the
/// snapshot importer service, which delegates to existing create commands where available.
/// </summary>
internal sealed partial class ImportWorkspaceSnapshotCommandHandler : ICommandHandler<ImportWorkspaceSnapshotCommand, SnapshotOutcome>
{
    private readonly IWorkspaceSnapshotImporter _importer;
    private readonly ILogger<ImportWorkspaceSnapshotCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImportWorkspaceSnapshotCommandHandler"/> class.
    /// </summary>
    /// <param name="importer">The snapshot importer service.</param>
    /// <param name="logger">The logger.</param>
    public ImportWorkspaceSnapshotCommandHandler(
        IWorkspaceSnapshotImporter importer,
        ILogger<ImportWorkspaceSnapshotCommandHandler> logger)
    {
        _importer = importer;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<SnapshotOutcome>> Handle(ImportWorkspaceSnapshotCommand request, CancellationToken cancellationToken)
    {
        var resourceCount = request.Snapshot.Resources.Values.Sum(_ => _.Count);
        LogHandling(resourceCount);

        try
        {
            var outcome = await _importer.ImportAsync(request.Snapshot, cancellationToken);
            LogImported(outcome.SuccessCount, outcome.FailureCount);
            return outcome;
        }
        catch (Exception ex)
        {
            LogFailed(ex.Message);
            return new Error($"Failed to import workspace: {ex.Message}");
        }
    }

    [LoggerMessage(LogLevel.Information, "Importing workspace snapshot with {ResourceCount} resources")]
    private partial void LogHandling(int resourceCount);

    [LoggerMessage(LogLevel.Information, "Workspace snapshot import completed: {SuccessCount} succeeded, {FailureCount} failed")]
    private partial void LogImported(int successCount, int failureCount);

    [LoggerMessage(LogLevel.Error, "Failed to import workspace snapshot: {Error}")]
    private partial void LogFailed(string error);
}
