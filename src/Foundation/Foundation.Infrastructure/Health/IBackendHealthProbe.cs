namespace Foundation.Infrastructure.Health;

/// <summary>
/// Performs the single sanctioned LocalStack-specific health call. All knowledge of the
/// LocalStack health endpoint and its service naming is isolated behind this abstraction
/// so that its failure affects nothing else (NFR-7.4).
/// </summary>
internal interface IBackendHealthProbe
{
    /// <summary>
    /// Probe the backend for per-service availability.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the probe.</param>
    /// <returns>The probe outcome expressed in catalogue service keys.</returns>
    Task<BackendHealthResult> ProbeAsync(CancellationToken cancellationToken);
}
