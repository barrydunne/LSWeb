using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Foundation.Infrastructure.Search;

/// <summary>
/// Background service that rebuilds the global search index roughly every 30 seconds by fully
/// re-enumerating every service source, then publishes the result into the <see cref="IndexStore"/>
/// with a single atomic swap. A manual refresh request wakes the loop before its next interval. The
/// build is fully isolated: any failure of an individual rebuild is swallowed so the previous
/// snapshot is retained and the loop keeps running (cold &#8594; live, NFRX-CAP-2).
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Background rebuild loop exercised via integration tests.")]
internal sealed partial class SearchIndexer : BackgroundService
{
    private static readonly TimeSpan _rebuildInterval = TimeSpan.FromSeconds(30);

    private readonly SearchIndexBuilder _builder;
    private readonly IndexStore _store;
    private readonly SearchIndexCoordinator _coordinator;
    private readonly ILogger _logger;

    public SearchIndexer(
        SearchIndexBuilder builder,
        IndexStore store,
        SearchIndexCoordinator coordinator,
        ILogger<SearchIndexer> logger)
    {
        _builder = builder;
        _store = store;
        _coordinator = coordinator;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await RebuildAsync(stoppingToken);
            try
            {
                await _coordinator.WaitForRefreshAsync(_rebuildInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task RebuildAsync(CancellationToken cancellationToken)
    {
        _coordinator.BeginBuild();
        try
        {
            _store.Replace(await _builder.BuildAsync(cancellationToken));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogRebuildFailed(exception);
        }
        finally
        {
            _coordinator.EndBuild();
        }
    }

    [LoggerMessage(LogLevel.Warning, "Search index rebuild failed.")]
    private partial void LogRebuildFailed(Exception exception);
}
