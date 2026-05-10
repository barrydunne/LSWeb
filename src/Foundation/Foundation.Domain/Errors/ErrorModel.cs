namespace Foundation.Domain.Errors;

/// <summary>
/// A normalised, user-facing representation of a backend failure. Raw SDK exceptions are never
/// surfaced across layers; they are translated into an <see cref="ErrorModel"/> instead.
/// </summary>
/// <param name="Code">A stable machine-readable code, for example the AWS error code.</param>
/// <param name="Message">A human-readable message safe to show to the user.</param>
/// <param name="Category">The normalised classification of the failure.</param>
/// <param name="Classification">Whether the failure is retryable or terminal.</param>
public sealed record ErrorModel(
    string Code,
    string Message,
    ErrorCategory Category,
    ErrorClassification Classification)
{
    /// <summary>
    /// Gets a value indicating whether the failure is eligible for a bounded retry.
    /// </summary>
    public bool IsRetryable => Classification == ErrorClassification.Retryable;
}
