using Foundation.Application.Health;
using Foundation.Domain.Health;

namespace Foundation.Infrastructure.Health;

/// <summary>
/// Thread-safe holder for the latest backend health snapshot. Seeded with an all-unknown
/// snapshot until the first probe completes.
/// </summary>
internal sealed class HealthStatusStore : IHealthStatusProvider
{
    private volatile HealthStatus _current = HealthSnapshotBuilder.Unknown();

    public HealthStatus GetCurrent() => _current;

    /// <summary>
    /// Replace the current snapshot with a newly observed one.
    /// </summary>
    /// <param name="status">The new snapshot.</param>
    public void Update(HealthStatus status) => _current = status;
}
