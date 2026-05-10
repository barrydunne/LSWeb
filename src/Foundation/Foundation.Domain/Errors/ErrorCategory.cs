namespace Foundation.Domain.Errors;

/// <summary>
/// A normalised classification of a backend failure, derived from the underlying SDK error.
/// </summary>
public enum ErrorCategory
{
    /// <summary>
    /// The backend rejected the request because it was rate-limited or throttled.
    /// </summary>
    Throttling,

    /// <summary>
    /// A temporary backend or connectivity error that is expected to clear on retry.
    /// </summary>
    Transient,

    /// <summary>
    /// The request was malformed or failed validation.
    /// </summary>
    Validation,

    /// <summary>
    /// The requested resource does not exist.
    /// </summary>
    NotFound,

    /// <summary>
    /// The caller is not permitted to perform the operation.
    /// </summary>
    AccessDenied,

    /// <summary>
    /// The service or operation is not supported by the running backend.
    /// </summary>
    Unsupported,

    /// <summary>
    /// The failure could not be classified into a more specific category.
    /// </summary>
    Unknown,
}
