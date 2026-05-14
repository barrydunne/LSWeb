namespace Foundation.Domain.Search;

/// <summary>
/// A single searchable item in the global resource search index. Carries enough information to
/// match a query and to render a result that navigates to the underlying resource.
/// </summary>
/// <param name="ServiceKey">The catalogue key of the owning service, for example <c>sqs</c>.</param>
/// <param name="ResourceId">The bare identifier matched against the query, for example a queue name.</param>
/// <param name="DisplayName">The human-readable label shown for the result.</param>
/// <param name="Route">The relative SPA route that opens the matched resource.</param>
public sealed record SearchEntry(
    string ServiceKey,
    string ResourceId,
    string DisplayName,
    string Route);
