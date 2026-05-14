using System.Collections.Concurrent;
using Foundation.Domain.Search;
using Microsoft.Extensions.Logging;

namespace Foundation.Infrastructure.Search;

/// <summary>
/// Builds a global search index snapshot by enumerating every registered <see cref="IResourceSource"/>
/// with bounded parallelism. Each source is isolated: if one faults, its failure is logged and it
/// contributes nothing, while the remaining sources still fold into the snapshot. The resulting
/// entries are ordered deterministically so equal inputs always yield an equal snapshot.
/// </summary>
internal sealed partial class SearchIndexBuilder
{
    private const int MaxDegreeOfParallelism = 4;

    private readonly IReadOnlyList<IResourceSource> _sources;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger _logger;

    public SearchIndexBuilder(
        IEnumerable<IResourceSource> sources,
        TimeProvider timeProvider,
        ILogger<SearchIndexBuilder> logger)
    {
        _sources = [.. sources];
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <summary>
    /// Enumerate every source and fold the discovered resources into a new snapshot.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while building.</param>
    /// <returns>The newly built snapshot, stamped with the current time.</returns>
    public async Task<SearchIndexState> BuildAsync(CancellationToken cancellationToken)
    {
        var collected = new ConcurrentBag<SearchEntry>();

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = MaxDegreeOfParallelism,
            CancellationToken = cancellationToken,
        };

        await Parallel.ForEachAsync(
            _sources,
            options,
            async (source, token) => await AppendSourceAsync(source, collected, token));

        var entries = collected
            .OrderBy(_ => _.ServiceKey, StringComparer.Ordinal)
            .ThenBy(_ => _.ResourceId, StringComparer.Ordinal)
            .ToList();

        return new SearchIndexState(entries, _timeProvider.GetUtcNow());
    }

    private async Task AppendSourceAsync(
        IResourceSource source,
        ConcurrentBag<SearchEntry> collected,
        CancellationToken cancellationToken)
    {
        try
        {
            var entries = await source.ListAsync(cancellationToken);
            foreach (var entry in entries)
                collected.Add(entry);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogSourceFailed(source.ServiceKey, exception);
        }
    }

    [LoggerMessage(LogLevel.Warning, "Search index source for {ServiceKey} failed and was skipped.")]
    private partial void LogSourceFailed(string serviceKey, Exception exception);
}
