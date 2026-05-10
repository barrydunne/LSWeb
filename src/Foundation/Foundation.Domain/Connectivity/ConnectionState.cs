namespace Foundation.Domain.Connectivity;

/// <summary>
/// The resolved connectivity state for the configured AWS endpoint.
/// </summary>
/// <param name="Status">Whether the endpoint is reachable.</param>
/// <param name="Endpoint">The resolved service endpoint URL.</param>
/// <param name="Region">The resolved AWS region.</param>
/// <param name="Error">A human-readable error when the endpoint is unreachable; otherwise <see langword="null"/>.</param>
public sealed record ConnectionState(ConnectivityStatus Status, string Endpoint, string Region, string? Error);
