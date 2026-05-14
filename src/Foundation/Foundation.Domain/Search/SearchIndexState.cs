namespace Foundation.Domain.Search;

/// <summary>
/// An immutable snapshot of the global search index. Each rebuild produces a brand-new snapshot
/// that replaces the previous one in a single atomic swap, so a reader always observes a complete,
/// internally consistent set of entries and never a partially built index.
/// </summary>
/// <param name="Entries">The indexed entries, ordered as produced by the most recent rebuild.</param>
/// <param name="BuiltAt">When the snapshot was produced.</param>
public sealed record SearchIndexState(
    IReadOnlyList<SearchEntry> Entries,
    DateTimeOffset BuiltAt)
{
    /// <summary>
    /// An empty snapshot used before the first rebuild completes.
    /// </summary>
    public static SearchIndexState Empty { get; } = new([], DateTimeOffset.MinValue);

    /// <summary>
    /// The number of entries in the snapshot.
    /// </summary>
    public int Count => Entries.Count;
}
