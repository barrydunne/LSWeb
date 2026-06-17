namespace Foundation.Api.Models;

/// <summary>
/// The AWS gateway circuit-breaker status.
/// </summary>
/// <param name="IsOpen">Whether at least one service has an open circuit breaker.</param>
/// <param name="AffectedServices">The keys of services whose calls are currently being rejected, in key order.</param>
public sealed record CircuitStatusResponse(bool IsOpen, IReadOnlyList<string> AffectedServices);
