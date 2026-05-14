using Foundation.Application.Search;

namespace Foundation.Infrastructure.Search;

/// <summary>
/// Coordinates the background search indexer with the API surface. Tracks whether a rebuild is in
/// progress (surfaced through <see cref="ISearchIndexSignals"/>) and lets callers request an
/// out-of-band rebuild (through <see cref="ISearchRefreshTrigger"/>) that wakes the indexer before
/// its next scheduled interval. A single pending refresh is coalesced so repeated requests do not
/// queue up multiple rebuilds.
/// </summary>
internal sealed class SearchIndexCoordinator : ISearchIndexSignals, ISearchRefreshTrigger, IDisposable
{
    private readonly SemaphoreSlim _refreshSignal = new(0, 1);
    private volatile bool _isBuilding;

    public bool IsBuilding => _isBuilding;

    public void RequestRefresh()
    {
        try
        {
            _refreshSignal.Release();
        }
        catch (SemaphoreFullException)
        {
            // A refresh is already pending; coalesce this request into the existing one.
        }
    }

    /// <summary>
    /// Mark the start of a rebuild so that <see cref="IsBuilding"/> reports <see langword="true"/>.
    /// </summary>
    public void BeginBuild() => _isBuilding = true;

    /// <summary>
    /// Mark the end of a rebuild so that <see cref="IsBuilding"/> reports <see langword="false"/>.
    /// </summary>
    public void EndBuild() => _isBuilding = false;

    /// <summary>
    /// Wait for either an out-of-band refresh request or the supplied interval to elapse.
    /// </summary>
    /// <param name="interval">The maximum time to wait before rebuilding on schedule.</param>
    /// <param name="cancellationToken">A token to cancel the wait.</param>
    /// <returns><see langword="true"/> if a refresh was requested; <see langword="false"/> if the interval elapsed.</returns>
    public Task<bool> WaitForRefreshAsync(TimeSpan interval, CancellationToken cancellationToken)
        => _refreshSignal.WaitAsync(interval, cancellationToken);

    public void Dispose() => _refreshSignal.Dispose();
}
