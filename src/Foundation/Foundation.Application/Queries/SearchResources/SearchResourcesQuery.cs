using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Search;

namespace Foundation.Application.Queries.SearchResources;

/// <summary>
/// Query the most recently built search index for resources matching a free-text term.
/// </summary>
/// <param name="Query">The free-text term to match against indexed resources.</param>
public record SearchResourcesQuery(string Query) : IQuery<SearchResourcesQueryResult>;

/// <summary>
/// The result of a resource search.
/// </summary>
/// <param name="Matches">The indexed entries that matched the query, in index order.</param>
public record SearchResourcesQueryResult(IReadOnlyList<SearchEntry> Matches);
