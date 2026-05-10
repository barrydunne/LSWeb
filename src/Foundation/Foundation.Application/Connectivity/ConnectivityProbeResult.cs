namespace Foundation.Application.Connectivity;

/// <summary>
/// The outcome of a backend reachability probe.
/// </summary>
/// <param name="IsReachable">Whether the backend responded successfully.</param>
/// <param name="Error">A human-readable error when the backend is unreachable; otherwise <see langword="null"/>.</param>
public sealed record ConnectivityProbeResult(bool IsReachable, string? Error);
