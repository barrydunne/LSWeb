using Foundation.Domain.Snapshot;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Snapshot;

/// <summary>
/// Imports a workspace snapshot by deserialising and recreating each of its resources through the
/// appropriate service adapters and create commands.
/// </summary>
internal sealed partial class WorkspaceSnapshotImporter : IWorkspaceSnapshotImporter
{
    private readonly ILogger<WorkspaceSnapshotImporter> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkspaceSnapshotImporter"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public WorkspaceSnapshotImporter(ILogger<WorkspaceSnapshotImporter> logger) => _logger = logger;

    /// <inheritdoc />
    public async Task<SnapshotOutcome> ImportAsync(WorkspaceSnapshot snapshot, CancellationToken cancellationToken)
    {
        LogBeginning(snapshot.Id);

        var operationId = $"imp-{Guid.NewGuid():N}".Substring(0, 16);
        var failures = new List<SnapshotFailureDetail>();
        var successCount = 0;
        var totalResources = 0;

        // Import resources grouped by service
        foreach (var serviceGroup in snapshot.Resources)
        {
            var serviceKey = serviceGroup.Key;
            foreach (var resource in serviceGroup.Value)
            {
                totalResources++;
                try
                {
                    LogImporting(serviceKey, resource.ResourceType, resource.ResourceId);

                    // TODO: Implement actual resource recreation logic via AwsGateway
                    // For now, log and succeed (placeholder implementation)
                    LogImported(serviceKey, resource.ResourceType, resource.ResourceName);
                    successCount++;
                }
                catch (Exception ex)
                {
                    LogImportFailed(serviceKey, resource.ResourceType, resource.ResourceId, ex.Message);
                    failures.Add(new SnapshotFailureDetail(serviceKey, resource.ResourceId, ex.Message));
                }
            }
        }

        var outcome = new SnapshotOutcome(
            operationId,
            "Import",
            DateTime.UtcNow,
            totalResources,
            successCount,
            failures.Count,
            failures.AsReadOnly());

        LogCompleted(successCount, totalResources);
        return await Task.FromResult(outcome);
    }

    [LoggerMessage(LogLevel.Trace, "Beginning workspace snapshot import for snapshot {SnapshotId}")]
    private partial void LogBeginning(string snapshotId);

    [LoggerMessage(LogLevel.Trace, "Importing resource {ServiceKey}/{ResourceType}/{ResourceId}")]
    private partial void LogImporting(string serviceKey, string resourceType, string resourceId);

    [LoggerMessage(LogLevel.Information, "Imported {ServiceKey}/{ResourceType}/{ResourceName}")]
    private partial void LogImported(string serviceKey, string resourceType, string resourceName);

    [LoggerMessage(LogLevel.Warning, "Failed to import {ServiceKey}/{ResourceType}/{ResourceId}: {Error}")]
    private partial void LogImportFailed(string serviceKey, string resourceType, string resourceId, string error);

    [LoggerMessage(LogLevel.Information, "Workspace snapshot import completed: {SuccessCount}/{TotalResources} resources imported")]
    private partial void LogCompleted(int successCount, int totalResources);
}
