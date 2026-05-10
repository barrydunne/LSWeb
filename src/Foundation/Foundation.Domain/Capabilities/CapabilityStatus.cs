namespace Foundation.Domain.Capabilities;

/// <summary>
/// Whether the running backend supports a given service or operation.
/// </summary>
public enum CapabilityStatus
{
    /// <summary>
    /// The backend is known to support the service or operation.
    /// </summary>
    Supported,

    /// <summary>
    /// The backend is known not to support the service or operation.
    /// </summary>
    Unsupported,

    /// <summary>
    /// Support has not yet been determined.
    /// </summary>
    Unknown,
}
