using AspNet.KickStarter.CQRS.Abstractions.Queries;

namespace Foundation.Application.Queries.GetDiagnostics;

/// <summary>
/// Query the diagnostic snapshot of the resolved configuration and backend connectivity.
/// </summary>
/// <param name="Reveal">Whether the caller has explicitly requested sensitive values to be unmasked.</param>
public record GetDiagnosticsQuery(bool Reveal) : IQuery<GetDiagnosticsQueryResult>;

/// <summary>
/// The result of a diagnostics query.
/// </summary>
/// <param name="Configuration">The resolved configuration values with sensitive values redacted unless revealed.</param>
/// <param name="Endpoint">The resolved service endpoint URL.</param>
/// <param name="Region">The resolved AWS region.</param>
/// <param name="ConnectivityStatus">The backend connectivity status, either <c>Connected</c> or <c>Disconnected</c>.</param>
/// <param name="ConnectivityError">A human-readable error when disconnected; otherwise <see langword="null"/>.</param>
/// <param name="RevealAllowed">Whether the host permits sensitive values to be revealed.</param>
public record GetDiagnosticsQueryResult(
    IReadOnlyList<DiagnosticsConfigValue> Configuration,
    string Endpoint,
    string Region,
    string ConnectivityStatus,
    string? ConnectivityError,
    bool RevealAllowed);

/// <summary>
/// A single resolved configuration value within a diagnostics snapshot.
/// </summary>
/// <param name="Name">The logical name of the configuration value.</param>
/// <param name="Value">The display value, redacted when sensitive and not revealed.</param>
/// <param name="Source">Where the resolved value originated, either <c>EnvironmentVariable</c> or <c>Default</c>.</param>
/// <param name="IsSensitive">Whether the value is sensitive and masked by default.</param>
public record DiagnosticsConfigValue(string Name, string Value, string Source, bool IsSensitive);
