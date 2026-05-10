namespace Foundation.Domain.Health;

/// <summary>
/// Describes whether a backend service is currently usable.
/// </summary>
public enum ServiceAvailability
{
    /// <summary>
    /// The service is enabled and reachable.
    /// </summary>
    Available,

    /// <summary>
    /// The service is known to be disabled or unreachable.
    /// </summary>
    Unavailable,

    /// <summary>
    /// The service availability could not be determined.
    /// </summary>
    Unknown,
}
