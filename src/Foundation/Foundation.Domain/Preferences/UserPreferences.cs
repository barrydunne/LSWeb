namespace Foundation.Domain.Preferences;

/// <summary>
/// The persisted, user-scoped preferences retained outside the container so that they survive
/// restarts when a host directory is mounted. The collections hold resource references in the
/// order the user arranged or visited them.
/// </summary>
/// <param name="Favourites">Resource references the user pinned for quick access, most important first.</param>
/// <param name="RecentlyViewed">Resource references the user opened recently, most recent first.</param>
public sealed record UserPreferences(
    IReadOnlyList<string> Favourites,
    IReadOnlyList<string> RecentlyViewed)
{
    /// <summary>
    /// Gets an empty set of preferences, used as the default before anything has been saved.
    /// </summary>
    public static UserPreferences Empty { get; } = new([], []);

    /// <summary>
    /// Record that a resource was opened, moving it to the front of the recently-viewed list,
    /// removing any earlier occurrence, and trimming the list to <paramref name="maxItems"/> entries.
    /// </summary>
    /// <param name="reference">The resource reference that was opened.</param>
    /// <param name="maxItems">The maximum number of recently-viewed references to retain.</param>
    /// <returns>A new <see cref="UserPreferences"/> with the updated recently-viewed list.</returns>
    public UserPreferences WithRecentlyViewed(string reference, int maxItems)
    {
        var updated = new List<string>(RecentlyViewed.Count + 1) { reference };
        updated.AddRange(RecentlyViewed.Where(_ => _ != reference));
        if (updated.Count > maxItems)
            updated.RemoveRange(maxItems, updated.Count - maxItems);

        return this with { RecentlyViewed = updated };
    }

    /// <summary>
    /// Pin a resource as a favourite, appending it when it is not already pinned so that the
    /// existing order is preserved.
    /// </summary>
    /// <param name="reference">The resource reference to pin.</param>
    /// <returns>A new <see cref="UserPreferences"/> including the favourite, or the same instance when already pinned.</returns>
    public UserPreferences WithFavourite(string reference)
        => Favourites.Contains(reference)
            ? this
            : this with { Favourites = [.. Favourites, reference] };

    /// <summary>
    /// Unpin a resource, removing it from the favourites list when present.
    /// </summary>
    /// <param name="reference">The resource reference to unpin.</param>
    /// <returns>A new <see cref="UserPreferences"/> without the favourite.</returns>
    public UserPreferences WithoutFavourite(string reference)
        => this with { Favourites = [.. Favourites.Where(_ => _ != reference)] };
}
