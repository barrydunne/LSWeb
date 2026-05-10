namespace Foundation.Domain.Errors;

/// <summary>
/// Whether a failure should be retried by the resilience pipeline or surfaced immediately.
/// </summary>
public enum ErrorClassification
{
    /// <summary>
    /// The failure is transient and may succeed on a bounded retry.
    /// </summary>
    Retryable,

    /// <summary>
    /// The failure is permanent for this request and must surface to the caller immediately.
    /// </summary>
    Terminal,
}
