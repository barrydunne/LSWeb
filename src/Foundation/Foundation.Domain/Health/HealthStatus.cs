namespace Foundation.Domain.Health;

/// <summary>
/// The overall backend health snapshot, describing the availability of each catalogue service.
/// </summary>
/// <param name="Services">The per-service availability results.</param>
public sealed record HealthStatus(IReadOnlyList<ServiceHealth> Services);
