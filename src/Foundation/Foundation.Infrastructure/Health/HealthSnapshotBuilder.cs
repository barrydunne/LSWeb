using Foundation.Domain.Catalogue;
using Foundation.Domain.Health;

namespace Foundation.Infrastructure.Health;

/// <summary>
/// Builds a <see cref="HealthStatus"/> snapshot for every catalogue service from a probe
/// result. When the probe did not succeed, all services are assumed available (NFR-7.4).
/// </summary>
internal static class HealthSnapshotBuilder
{
    /// <summary>
    /// Build a snapshot covering every catalogue service from the supplied probe result.
    /// </summary>
    /// <param name="probe">The most recent probe outcome.</param>
    /// <returns>A snapshot describing the availability of each catalogue service.</returns>
    public static HealthStatus Build(BackendHealthResult probe)
        => new(ServiceCatalogue.Services
            .Select(_ => new ServiceHealth(_.Key, Resolve(probe, _.Key)))
            .ToList());

    /// <summary>
    /// Build the initial snapshot where every catalogue service is of unknown availability.
    /// </summary>
    /// <returns>A snapshot marking every catalogue service as <see cref="ServiceAvailability.Unknown"/>.</returns>
    public static HealthStatus Unknown()
        => new(ServiceCatalogue.Services
            .Select(_ => new ServiceHealth(_.Key, ServiceAvailability.Unknown))
            .ToList());

    private static ServiceAvailability Resolve(BackendHealthResult probe, string key)
        => !probe.ProbeSucceeded || probe.AvailableServiceKeys.Contains(key)
            ? ServiceAvailability.Available
            : ServiceAvailability.Unavailable;
}
