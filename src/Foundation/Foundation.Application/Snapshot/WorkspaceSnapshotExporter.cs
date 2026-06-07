using System.Text.Json;
using Foundation.Domain.Search;
using Foundation.Domain.Snapshot;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Snapshot;

/// <summary>
/// Exports the current workspace state by gathering all discoverable resources via the search index
/// and serialising them into a snapshot for download or archival.
/// </summary>
internal sealed partial class WorkspaceSnapshotExporter : IWorkspaceSnapshotExporter
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    private readonly ISearchIndexStore _searchIndex;
    private readonly ILogger<WorkspaceSnapshotExporter> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkspaceSnapshotExporter"/> class.
    /// </summary>
    /// <param name="searchIndex">The search index containing all current resources.</param>
    /// <param name="logger">The logger.</param>
    public WorkspaceSnapshotExporter(ISearchIndexStore searchIndex, ILogger<WorkspaceSnapshotExporter> logger)
    {
        _searchIndex = searchIndex;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<WorkspaceSnapshot> ExportAsync(CancellationToken cancellationToken)
    {
        LogBeginning();
        var snapshotId = $"snap-{Guid.NewGuid():N}".Substring(0, 16);
        var resources = new Dictionary<string, List<SnapshotResourceData>>();

        var current = _searchIndex.GetCurrent();
        if (current == null)
        {
            LogEmpty();
            var emptyDict = new Dictionary<string, IReadOnlyList<SnapshotResourceData>>();
            return new WorkspaceSnapshot(snapshotId, DateTime.UtcNow, emptyDict);
        }

        // Group all entries by service key
        // TODO: Integrate with full resource store to capture complete metadata for each resource
        foreach (var entry in current.Entries)
        {
            if (!resources.TryGetValue(entry.ServiceKey, out var serviceList))
            {
                serviceList = [];
                resources[entry.ServiceKey] = serviceList;
            }

            // Create a snapshot entry with available information
            // Full resource data would be fetched from service-specific stores
            var metadata = new { displayName = entry.DisplayName, route = entry.Route };
            var serialized = new SnapshotResourceData(
                entry.ServiceKey,
                "Resource",  // Type is not yet available from SearchEntry
                entry.ResourceId,
                entry.DisplayName,
                JsonSerializer.Serialize(metadata, JsonOptions));

            serviceList.Add(serialized);
        }

        var resourceCount = resources.Values.Sum(_ => _.Count);
        var readOnlyDict = resources.ToDictionary(_ => _.Key, _ => (IReadOnlyList<SnapshotResourceData>)_.Value.AsReadOnly());
        var snapshot = new WorkspaceSnapshot(snapshotId, DateTime.UtcNow, readOnlyDict);

        LogCompleted(resourceCount);
        return await Task.FromResult(snapshot);
    }

    [LoggerMessage(LogLevel.Trace, "Beginning workspace snapshot export")]
    private partial void LogBeginning();

    [LoggerMessage(LogLevel.Warning, "Search index is empty; creating snapshot with no resources")]
    private partial void LogEmpty();

    [LoggerMessage(LogLevel.Information, "Workspace snapshot export completed with {ResourceCount} total resources")]
    private partial void LogCompleted(int resourceCount);
}
