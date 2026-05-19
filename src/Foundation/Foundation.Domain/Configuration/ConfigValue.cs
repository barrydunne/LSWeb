namespace Foundation.Domain.Configuration;

/// <summary>
/// A single resolved configuration value together with its origin and sensitivity.
/// </summary>
/// <param name="Name">The logical name of the configuration value.</param>
/// <param name="Value">The resolved value.</param>
/// <param name="Source">Where the resolved value originated.</param>
/// <param name="IsSensitive">Whether the value is sensitive and must be masked when displayed.</param>
public sealed record ConfigValue(string Name, string Value, ConfigSource Source, bool IsSensitive)
{
    /// <summary>
    /// The sentinel returned in place of a sensitive value when it is masked.
    /// </summary>
    public const string Mask = "********";

    /// <summary>
    /// Gets a representation of the value that is safe to display, masking sensitive values.
    /// </summary>
    public string Display => IsSensitive ? Mask : Value;
}
