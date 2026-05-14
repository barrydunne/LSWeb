namespace Foundation.Api.Models;

/// <summary>
/// The current state of the background search index.
/// </summary>
/// <param name="BuiltAt">When the current snapshot was built.</param>
/// <param name="EntryCount">The number of entries in the current snapshot.</param>
/// <param name="IsBuilding">Whether a rebuild is currently in progress.</param>
public sealed record SearchStateResponse(
    DateTimeOffset BuiltAt,
    int EntryCount,
    bool IsBuilding);
