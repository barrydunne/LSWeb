using System.Collections.Concurrent;
using Foundation.Application.Resilience;
using Foundation.Domain.Resilience;

namespace Foundation.Infrastructure.Resilience;

/// <summary>
/// Tracks the services whose AWS calls are currently being rejected by an open circuit breaker.
/// A service is marked suspended when the gateway observes a rejected call and cleared when a
/// later call to that service succeeds, so the surfaced state auto-recovers once the breaker closes.
/// </summary>
internal sealed class CircuitBreakerMonitor : ICircuitBreakerMonitor, ICircuitBreakerStateProvider
{
    private readonly ConcurrentDictionary<string, byte> _suspended =
        new(StringComparer.Ordinal);

    /// <inheritdoc />
    public void RecordSuspended(string serviceKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceKey);
        _suspended[serviceKey] = 0;
    }

    /// <inheritdoc />
    public void RecordRecovered(string serviceKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceKey);
        _suspended.TryRemove(serviceKey, out _);
    }

    /// <inheritdoc />
    public CircuitStatus GetStatus()
    {
        var affected = _suspended.Keys
            .OrderBy(key => key, StringComparer.Ordinal)
            .ToList();
        return new CircuitStatus(affected.Count > 0, affected);
    }
}
