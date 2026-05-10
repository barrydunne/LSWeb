using Foundation.Domain.Errors;

namespace Foundation.Infrastructure.Errors;

/// <summary>
/// Translates raw AWS SDK exceptions into a normalised <see cref="ErrorModel"/> so failures can
/// be surfaced and classified consistently without leaking SDK types across layers.
/// </summary>
internal interface IErrorTranslator
{
    /// <summary>
    /// Translates an exception into a normalised error model.
    /// </summary>
    /// <param name="exception">The exception thrown by an AWS SDK operation.</param>
    /// <returns>The normalised error model describing the failure.</returns>
    ErrorModel Translate(Exception exception);
}
