using Foundation.Domain.Health;

namespace Foundation.Application.Health;

/// <summary>
/// Provides access to the most recently observed backend health snapshot.
/// </summary>
public interface IHealthStatusProvider
{
    /// <summary>
    /// Get the latest backend health snapshot.
    /// </summary>
    /// <returns>The current <see cref="HealthStatus"/>.</returns>
    HealthStatus GetCurrent();
}
