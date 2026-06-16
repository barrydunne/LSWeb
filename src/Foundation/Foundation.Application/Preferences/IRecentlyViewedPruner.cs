using AspNet.KickStarter.FunctionalResult;

namespace Foundation.Application.Preferences;

/// <summary>
/// Removes recently-viewed resource references that no longer correspond to a resource present in
/// the current search index, so the recently-viewed list does not accumulate stale entries after a
/// resource is deleted or the backend is recreated from scratch.
/// </summary>
public interface IRecentlyViewedPruner
{
    /// <summary>
    /// Prune the recently-viewed list of any references whose resources can no longer be found in
    /// the current search index, persisting the trimmed list only when something was removed. This
    /// no-ops while the search index has not yet completed its first build, and leaves references
    /// that cannot be resolved to a known service untouched.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the operation.</param>
    /// <returns>A successful result, or a failure describing why pruning could not complete.</returns>
    Task<Result> PruneAsync(CancellationToken cancellationToken);
}
