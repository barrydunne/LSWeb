using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Navigation;
using Foundation.Application.Search;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Preferences;

/// <summary>
/// Reconciles the recently-viewed list against the latest search index snapshot, dropping any
/// reference whose resource is no longer indexed. References that cannot be resolved to a known
/// service are retained, since their absence from the index is not positive proof the resource is
/// gone. Pruning is skipped entirely until the index has completed its first build so a cold start
/// can never wipe the list against an empty index.
/// </summary>
internal sealed partial class RecentlyViewedPruner : IRecentlyViewedPruner
{
    private readonly IUserDataStore _userDataStore;
    private readonly ISearchIndexProvider _searchIndexProvider;
    private readonly IReferenceResolver _referenceResolver;
    private readonly ILogger _logger;

    public RecentlyViewedPruner(
        IUserDataStore userDataStore,
        ISearchIndexProvider searchIndexProvider,
        IReferenceResolver referenceResolver,
        ILogger<RecentlyViewedPruner> logger)
    {
        _userDataStore = userDataStore;
        _searchIndexProvider = searchIndexProvider;
        _referenceResolver = referenceResolver;
        _logger = logger;
    }

    public async Task<Result> PruneAsync(CancellationToken cancellationToken)
    {
        var index = _searchIndexProvider.GetCurrent();
        if (index.BuiltAt == DateTimeOffset.MinValue)
        {
            LogIndexNotReady();
            return Result.Success();
        }

        var preferences = await _userDataStore.GetPreferencesAsync(cancellationToken);
        if (!preferences.IsSuccess)
            return preferences.Error!.Value;

        var recentlyViewed = preferences.Value.RecentlyViewed;
        if (recentlyViewed.Count == 0)
            return Result.Success();

        var existing = new HashSet<(string ServiceKey, string ResourceId)>();
        foreach (var entry in index.Entries)
            existing.Add((entry.ServiceKey, entry.ResourceId));

        var retained = recentlyViewed.Where(reference => ShouldRetain(reference, existing)).ToList();
        if (retained.Count == recentlyViewed.Count)
            return Result.Success();

        LogPruned(recentlyViewed.Count - retained.Count);
        var updated = preferences.Value with { RecentlyViewed = retained };
        return await _userDataStore.SavePreferencesAsync(updated, cancellationToken);
    }

    /// <summary>
    /// Decide whether a recently-viewed reference should be kept. A reference that resolves to a
    /// known service is kept only when a matching resource is present in the index; a reference that
    /// cannot be resolved is left untouched.
    /// </summary>
    /// <param name="reference">The recently-viewed reference to evaluate.</param>
    /// <param name="existing">The set of service-key and resource-id pairs currently in the index.</param>
    /// <returns><see langword="true"/> to keep the reference; otherwise <see langword="false"/>.</returns>
    private bool ShouldRetain(string reference, HashSet<(string ServiceKey, string ResourceId)> existing)
    {
        var resolution = _referenceResolver.Resolve(reference);
        if (!resolution.IsSuccess)
            return true;

        var resolved = resolution.Value;
        return existing.Contains((resolved.ServiceKey, resolved.ResourceId));
    }

    [LoggerMessage(LogLevel.Trace, "Skipping recently-viewed pruning until the search index has been built.")]
    private partial void LogIndexNotReady();

    [LoggerMessage(LogLevel.Trace, "Pruned {Count} stale recently-viewed reference(s) from the recently-viewed list.")]
    private partial void LogPruned(int count);
}
