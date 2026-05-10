using Foundation.Domain.Capabilities;

namespace Foundation.Application.Capabilities;

/// <summary>
/// Provides the current capability snapshot describing which managed services the running
/// backend supports.
/// </summary>
public interface ICapabilityProvider
{
    /// <summary>
    /// Gets the current capability snapshot.
    /// </summary>
    /// <returns>The current <see cref="CapabilityMap"/>.</returns>
    CapabilityMap GetCapabilities();
}
