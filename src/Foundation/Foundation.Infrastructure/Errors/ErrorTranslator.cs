using System.Net;
using Amazon.Runtime;
using Foundation.Domain.Errors;

namespace Foundation.Infrastructure.Errors;

/// <summary>
/// Maps AWS SDK exceptions onto a normalised <see cref="ErrorModel"/>, classifying each failure
/// as retryable or terminal so the resilience pipeline and the UI can react consistently.
/// </summary>
internal sealed class ErrorTranslator : IErrorTranslator
{
    private static readonly HashSet<string> _throttlingCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Throttling",
        "ThrottlingException",
        "ThrottledException",
        "TooManyRequestsException",
        "RequestLimitExceeded",
        "RequestThrottled",
        "RequestThrottledException",
        "ProvisionedThroughputExceededException",
        "TransactionInProgressException",
        "SlowDown",
    };

    private static readonly HashSet<string> _accessDeniedCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "AccessDenied",
        "AccessDeniedException",
        "UnauthorizedOperation",
        "AuthorizationError",
    };

    private static readonly HashSet<string> _unsupportedCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "NotImplemented",
        "NotImplementedError",
        "InternalFailure",
        "UnsupportedOperation",
        "UnsupportedOperationException",
        "InvalidAction",
        "MethodNotAllowed",
    };

    /// <inheritdoc />
    public ErrorModel Translate(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            AmazonServiceException serviceException => TranslateServiceException(serviceException),
            AmazonClientException => Transient("ClientError"),
            HttpRequestException => Transient("HttpRequestError"),
            TimeoutException => Transient("Timeout"),
            _ => new ErrorModel(
                exception.GetType().Name,
                MessageFor(ErrorCategory.Unknown),
                ErrorCategory.Unknown,
                ErrorClassification.Terminal),
        };
    }

    private static ErrorModel TranslateServiceException(AmazonServiceException exception)
    {
        var code = string.IsNullOrWhiteSpace(exception.ErrorCode)
            ? exception.GetType().Name
            : exception.ErrorCode;
        var category = Categorize(exception, code);
        var classification = category is ErrorCategory.Throttling or ErrorCategory.Transient
            ? ErrorClassification.Retryable
            : ErrorClassification.Terminal;

        return new ErrorModel(code, MessageFor(category), category, classification);
    }

    private static ErrorCategory Categorize(AmazonServiceException exception, string code)
    {
        if (exception.StatusCode == HttpStatusCode.NotImplemented
            || _unsupportedCodes.Contains(code)
            || MentionsUnsupported(exception.Message))
        {
            return ErrorCategory.Unsupported;
        }

        if (exception.StatusCode == HttpStatusCode.TooManyRequests || _throttlingCodes.Contains(code))
        {
            return ErrorCategory.Throttling;
        }

        if (exception.ErrorType == ErrorType.Receiver || IsServerError(exception.StatusCode))
        {
            return ErrorCategory.Transient;
        }

        if (exception.StatusCode == HttpStatusCode.NotFound
            || code.Contains("NotFound", StringComparison.OrdinalIgnoreCase))
        {
            return ErrorCategory.NotFound;
        }

        if (exception.StatusCode == HttpStatusCode.Forbidden
            || exception.StatusCode == HttpStatusCode.Unauthorized
            || _accessDeniedCodes.Contains(code))
        {
            return ErrorCategory.AccessDenied;
        }

        if (exception.StatusCode == HttpStatusCode.BadRequest
            || code.Contains("Validation", StringComparison.OrdinalIgnoreCase)
            || code.Contains("InvalidParameter", StringComparison.OrdinalIgnoreCase)
            || code.Contains("MissingParameter", StringComparison.OrdinalIgnoreCase))
        {
            return ErrorCategory.Validation;
        }

        return ErrorCategory.Unknown;
    }

    private static bool MentionsUnsupported(string message)
        => message.Contains("not yet implemented", StringComparison.OrdinalIgnoreCase)
            || message.Contains("not implemented", StringComparison.OrdinalIgnoreCase)
            || message.Contains("not supported", StringComparison.OrdinalIgnoreCase);

    private static bool IsServerError(HttpStatusCode statusCode)
        => (int)statusCode >= 500;

    private static ErrorModel Transient(string code)
        => new(code, MessageFor(ErrorCategory.Transient), ErrorCategory.Transient, ErrorClassification.Retryable);

    private static string MessageFor(ErrorCategory category)
        => category switch
        {
            ErrorCategory.Throttling => "The service is busy. The request will be retried.",
            ErrorCategory.Transient => "A temporary backend error occurred. The request will be retried.",
            ErrorCategory.Validation => "The request was not valid.",
            ErrorCategory.NotFound => "The requested resource was not found.",
            ErrorCategory.AccessDenied => "Access was denied for this operation.",
            ErrorCategory.Unsupported => "This service or operation is not supported by the current backend.",
            _ => "An unexpected error occurred.",
        };
}
