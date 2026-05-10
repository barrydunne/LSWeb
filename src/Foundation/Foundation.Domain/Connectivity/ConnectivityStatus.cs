namespace Foundation.Domain.Connectivity;

/// <summary>
/// Describes whether the backend AWS endpoint is reachable.
/// </summary>
public enum ConnectivityStatus
{
    /// <summary>
    /// The backend endpoint responded successfully.
    /// </summary>
    Connected,

    /// <summary>
    /// The backend endpoint could not be reached.
    /// </summary>
    Disconnected,
}
