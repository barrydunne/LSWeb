namespace Foundation.Domain.Configuration;

/// <summary>
/// Identifies where a resolved configuration value originated.
/// </summary>
public enum ConfigSource
{
    /// <summary>
    /// The value was supplied by an environment variable.
    /// </summary>
    EnvironmentVariable,

    /// <summary>
    /// The value was not supplied and a built-in default was applied.
    /// </summary>
    Default,
}
