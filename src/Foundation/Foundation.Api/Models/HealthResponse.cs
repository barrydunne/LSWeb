namespace Foundation.Api.Models;

/// <summary>
/// The backend health snapshot describing the availability of each managed service.
/// </summary>
/// <param name="Services">The per-service availability results in catalogue order.</param>
public sealed record HealthResponse(IReadOnlyList<ServiceHealthResponse> Services);

/// <summary>
/// The resolved availability of a single managed service.
/// </summary>
/// <param name="Key">The stable lowercase service identifier.</param>
/// <param name="Availability">The availability state: <c>Available</c>, <c>Unavailable</c>, or <c>Unknown</c>.</param>
public sealed record ServiceHealthResponse(string Key, string Availability);
