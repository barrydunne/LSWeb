using Foundation.Domain.Search;

namespace Foundation.Infrastructure.Search;

/// <summary>
/// Enumerates the searchable resources for a single AWS service so the search indexer can fold
/// them into the global index. Each managed service contributes one implementation; the indexer
/// fans out across every registered source with bounded parallelism and isolates failures so one
/// faulting service cannot abort the whole rebuild.
/// </summary>
internal interface IResourceSource
{
    /// <summary>
    /// Gets the catalogue key of the service this source enumerates (for example <c>sqs</c>).
    /// </summary>
    string ServiceKey { get; }

    /// <summary>
    /// List the current searchable resources for the service.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while listing.</param>
    /// <returns>The resources exposed as search entries; empty when the service has none.</returns>
    Task<IReadOnlyList<SearchEntry>> ListAsync(CancellationToken cancellationToken);
}
