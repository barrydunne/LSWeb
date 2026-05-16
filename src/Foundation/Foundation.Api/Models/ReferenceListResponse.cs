namespace Foundation.Api.Models;

/// <summary>
/// A list of resource references, such as recently-viewed resources or pinned favourites.
/// </summary>
/// <param name="References">The resource references.</param>
public sealed record ReferenceListResponse(IReadOnlyList<string> References);
