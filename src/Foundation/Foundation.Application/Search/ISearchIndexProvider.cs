using Foundation.Domain.Search;

namespace Foundation.Application.Search;

/// <summary>
/// Provides read access to the most recently built global search index snapshot.
/// </summary>
public interface ISearchIndexProvider
{
    /// <summary>
    /// Get the current search index snapshot.
    /// </summary>
    /// <returns>The current <see cref="SearchIndexState"/>.</returns>
    SearchIndexState GetCurrent();
}
