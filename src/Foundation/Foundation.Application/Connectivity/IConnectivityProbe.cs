namespace Foundation.Application.Connectivity;

/// <summary>
/// Performs a cheap reachability check against the configured AWS backend.
/// </summary>
public interface IConnectivityProbe
{
    /// <summary>
    /// Probe the backend endpoint for reachability.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the probe.</param>
    /// <returns>The probe outcome.</returns>
    Task<ConnectivityProbeResult> CheckAsync(CancellationToken cancellationToken);
}
