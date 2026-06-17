namespace Foundation.Domain.Resilience;

/// <summary>
/// A snapshot of the AWS gateway circuit-breaker state, describing whether the breaker is
/// currently open and which catalogue services have been observed rejecting calls while open.
/// </summary>
/// <param name="IsOpen">Whether at least one service has an open circuit breaker.</param>
/// <param name="AffectedServices">The catalogue keys of services whose calls are currently being rejected, in key order.</param>
public sealed record CircuitStatus(bool IsOpen, IReadOnlyList<string> AffectedServices);
