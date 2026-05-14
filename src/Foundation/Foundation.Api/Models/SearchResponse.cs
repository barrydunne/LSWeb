namespace Foundation.Api.Models;

/// <summary>
/// The results of a global resource search.
/// </summary>
/// <param name="Matches">The indexed resources that matched the query, grouped client-side by service.</param>
public sealed record SearchResponse(IReadOnlyList<SearchMatchResponse> Matches);

/// <summary>
/// A single resource that matched a search query.
/// </summary>
/// <param name="ServiceKey">The catalogue key of the owning service, for example <c>sqs</c>.</param>
/// <param name="ResourceId">The bare identifier of the matched resource.</param>
/// <param name="DisplayName">The human-readable label shown for the result.</param>
/// <param name="Route">The relative SPA route that opens the matched resource.</param>
public sealed record SearchMatchResponse(
    string ServiceKey,
    string ResourceId,
    string DisplayName,
    string Route);
