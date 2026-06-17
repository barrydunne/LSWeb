using Foundation.Domain.Resilience;

namespace Foundation.Application.Resilience;

/// <summary>
/// Provides access to the current AWS gateway circuit-breaker state.
/// </summary>
public interface ICircuitBreakerStateProvider
{
    /// <summary>
    /// Get the current circuit-breaker status.
    /// </summary>
    /// <returns>The current <see cref="CircuitStatus"/>.</returns>
    CircuitStatus GetStatus();
}
