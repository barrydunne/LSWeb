using System.Diagnostics.CodeAnalysis;
using Foundation.Application.Preferences;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Foundation.Infrastructure.Preferences;

/// <summary>
/// Background service that prunes stale entries from the recently-viewed list shortly after startup
/// and then periodically, by delegating to <see cref="IRecentlyViewedPruner"/>. An initial delay
/// gives the search index time to complete its first build before the first prune, and the pruner
/// itself no-ops until the index is ready, so an early run can never wipe the list. Each pass is
/// fully isolated: any failure is swallowed so the loop keeps running.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Background pruning loop exercised via integration tests.")]
internal sealed partial class RecentlyViewedPruneMonitor : BackgroundService
{
    private static readonly TimeSpan _initialDelay = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan _pruneInterval = TimeSpan.FromMinutes(5);

    private readonly IRecentlyViewedPruner _pruner;
    private readonly ILogger _logger;

    public RecentlyViewedPruneMonitor(IRecentlyViewedPruner pruner, ILogger<RecentlyViewedPruneMonitor> logger)
    {
        _pruner = pruner;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(_initialDelay, stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                await PruneAsync(stoppingToken);
                await Task.Delay(_pruneInterval, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // The host is shutting down; stop the loop.
        }
    }

    private async Task PruneAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _pruner.PruneAsync(cancellationToken);
            if (!result.IsSuccess)
                LogPruneFailed(result.Error!.Value.Message);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogPruneError(exception);
        }
    }

    [LoggerMessage(LogLevel.Warning, "Recently-viewed pruning reported a failure: {Error}")]
    private partial void LogPruneFailed(string error);

    [LoggerMessage(LogLevel.Warning, "Recently-viewed pruning threw an unexpected exception.")]
    private partial void LogPruneError(Exception exception);
}
