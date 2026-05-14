namespace Foundation.Application.Search;

/// <summary>
/// Reports the live status of the background search index rebuild loop.
/// </summary>
public interface ISearchIndexSignals
{
    /// <summary>
    /// Gets a value indicating whether a rebuild of the search index is currently in progress.
    /// </summary>
    bool IsBuilding { get; }
}
