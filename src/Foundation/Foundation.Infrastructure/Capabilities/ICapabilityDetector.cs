using Foundation.Domain.Capabilities;
using Foundation.Domain.Errors;

namespace Foundation.Infrastructure.Capabilities;

/// <summary>
/// Tracks which services the running backend supports, deriving the transient
/// <see cref="CapabilityMap"/> from observed operation outcomes so unsupported services can be
/// surfaced gracefully rather than crashing the console.
/// </summary>
internal interface ICapabilityDetector
{
    /// <summary>
    /// Records that an operation against a service completed, marking the service supported.
    /// </summary>
    /// <param name="serviceKey">The catalogue service key the operation targeted.</param>
    void RecordSuccess(string serviceKey);

    /// <summary>
    /// Records a failed operation, marking the service unsupported when the error indicates the
    /// backend does not implement it, and otherwise supported.
    /// </summary>
    /// <param name="serviceKey">The catalogue service key the operation targeted.</param>
    /// <param name="error">The normalised error describing the failure.</param>
    void RecordError(string serviceKey, ErrorModel error);

    /// <summary>
    /// Gets the current capability snapshot for all known services.
    /// </summary>
    /// <returns>The transient capability map.</returns>
    CapabilityMap GetCapabilities();
}
