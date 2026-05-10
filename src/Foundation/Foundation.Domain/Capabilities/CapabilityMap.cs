namespace Foundation.Domain.Capabilities;

/// <summary>
/// A transient snapshot of which services the running backend supports. Derived from probing
/// and operation outcomes, it is never persisted and is rebuilt as the backend is exercised.
/// </summary>
/// <param name="Entries">The capability of each known service.</param>
public sealed record CapabilityMap(IReadOnlyList<CapabilityEntry> Entries)
{
    /// <summary>
    /// Gets an empty capability map.
    /// </summary>
    public static CapabilityMap Empty { get; } = new([]);

    /// <summary>
    /// Finds the capability entry for a service key.
    /// </summary>
    /// <param name="key">The catalogue service key to look up.</param>
    /// <returns>The matching entry, or <see langword="null"/> when the key is unknown.</returns>
    public CapabilityEntry? Find(string key)
        => Entries.FirstOrDefault(entry => string.Equals(entry.Key, key, StringComparison.Ordinal));

    /// <summary>
    /// Determines whether the backend is known to support a service.
    /// </summary>
    /// <param name="key">The catalogue service key to test.</param>
    /// <returns><see langword="true"/> when the service is supported; otherwise <see langword="false"/>.</returns>
    public bool IsSupported(string key)
        => Find(key)?.Status == CapabilityStatus.Supported;
}
