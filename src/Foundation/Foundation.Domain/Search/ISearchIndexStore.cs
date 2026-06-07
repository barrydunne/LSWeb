namespace Foundation.Domain.Search;

/// <summary>
/// Provides read-only access to the current global search index state.
/// </summary>
public interface ISearchIndexStore
{
    /// <summary>
    /// Gets the current snapshot of the search index.
    /// </summary>
    /// <returns>A snapshot of the current search index state, or null if not yet initialized.</returns>
    SearchIndexState? GetCurrent();
}
