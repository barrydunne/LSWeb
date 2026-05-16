namespace Foundation.Api.Models;

/// <summary>
/// A diagnostic snapshot of the resolved configuration and backend connectivity.
/// </summary>
/// <param name="Configuration">The resolved configuration values with sensitive values masked unless revealed.</param>
/// <param name="Endpoint">The resolved service endpoint URL.</param>
/// <param name="Region">The resolved AWS region.</param>
/// <param name="ConnectivityStatus">The connectivity status, either <c>Connected</c> or <c>Disconnected</c>.</param>
/// <param name="ConnectivityError">A human-readable error when disconnected; otherwise <see langword="null"/>.</param>
/// <param name="RevealAllowed">Whether the host permits sensitive values to be revealed.</param>
public sealed record DiagnosticsResponse(
    IReadOnlyList<DiagnosticsConfigResponse> Configuration,
    string Endpoint,
    string Region,
    string ConnectivityStatus,
    string? ConnectivityError,
    bool RevealAllowed);

/// <summary>
/// A single resolved configuration value within a diagnostics snapshot.
/// </summary>
/// <param name="Name">The logical name of the configuration value.</param>
/// <param name="Value">The display value, masked when sensitive and not revealed.</param>
/// <param name="Source">Where the resolved value originated, either <c>EnvironmentVariable</c> or <c>Default</c>.</param>
/// <param name="IsSensitive">Whether the value is sensitive and masked by default.</param>
public sealed record DiagnosticsConfigResponse(string Name, string Value, string Source, bool IsSensitive);
