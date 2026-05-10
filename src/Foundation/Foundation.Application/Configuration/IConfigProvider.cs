using Foundation.Domain.Configuration;

namespace Foundation.Application.Configuration;

/// <summary>
/// Provides access to the resolved AWS connection configuration.
/// </summary>
public interface IConfigProvider
{
    /// <summary>
    /// Get the resolved configuration snapshot.
    /// </summary>
    /// <returns>The resolved <see cref="ConfigSnapshot"/>.</returns>
    ConfigSnapshot GetSnapshot();
}
