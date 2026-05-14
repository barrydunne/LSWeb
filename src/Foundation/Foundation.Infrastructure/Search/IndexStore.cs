using Foundation.Application.Search;
using Foundation.Domain.Search;

namespace Foundation.Infrastructure.Search;

/// <summary>
/// Thread-safe holder for the latest global search index snapshot. A rebuild replaces the snapshot
/// with a single atomic reference assignment, so readers never block and never observe a partially
/// built index. Seeded with an empty snapshot until the first rebuild completes.
/// </summary>
internal sealed class IndexStore : ISearchIndexProvider
{
    private volatile SearchIndexState _current = SearchIndexState.Empty;

    public SearchIndexState GetCurrent() => _current;

    /// <summary>
    /// Replace the current snapshot with a newly built one.
    /// </summary>
    /// <param name="state">The new snapshot.</param>
    public void Replace(SearchIndexState state) => _current = state;
}
