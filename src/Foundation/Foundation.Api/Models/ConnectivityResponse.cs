namespace Foundation.Api.Models;

/// <summary>
/// The connectivity status of the configured AWS backend.
/// </summary>
/// <param name="Status">The connectivity status, either <c>Connected</c> or <c>Disconnected</c>.</param>
/// <param name="Endpoint">The resolved service endpoint URL.</param>
/// <param name="Region">The resolved AWS region.</param>
/// <param name="Error">A human-readable error when disconnected; otherwise <see langword="null"/>.</param>
public sealed record ConnectivityResponse(string Status, string Endpoint, string Region, string? Error);
