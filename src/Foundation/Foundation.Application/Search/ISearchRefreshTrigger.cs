namespace Foundation.Application.Search;

/// <summary>
/// Requests that the background search indexer rebuild the index as soon as possible rather than
/// waiting for its next scheduled interval.
/// </summary>
public interface ISearchRefreshTrigger
{
    /// <summary>
    /// Signal the indexer to rebuild the search index immediately. The call returns without waiting
    /// for the rebuild to complete.
    /// </summary>
    void RequestRefresh();
}
