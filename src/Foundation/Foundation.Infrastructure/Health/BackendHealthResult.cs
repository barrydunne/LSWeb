namespace Foundation.Infrastructure.Health;

/// <summary>
/// The outcome of a single backend health probe, expressed in catalogue service keys.
/// </summary>
/// <param name="ProbeSucceeded">
/// Whether the LocalStack health endpoint was reached and parsed. When <c>false</c>
/// (real AWS or the call was unavailable) every service is assumed available (NFR-7.4).
/// </param>
/// <param name="AvailableServiceKeys">The catalogue keys reported as usable by the probe.</param>
internal sealed record BackendHealthResult(bool ProbeSucceeded, IReadOnlySet<string> AvailableServiceKeys);
