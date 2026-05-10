using System.Net;
using Amazon.Runtime;
using Foundation.Domain.Errors;
using Foundation.Infrastructure.Errors;

namespace Foundation.UnitTests.Infrastructure.Errors;

public class ErrorTranslatorTests
{
    private readonly ErrorTranslator _sut = new();

    private static AmazonServiceException ServiceException(
        HttpStatusCode statusCode,
        string? errorCode = null,
        ErrorType errorType = ErrorType.Unknown,
        string message = "boom")
        => new(message) { StatusCode = statusCode, ErrorCode = errorCode!, ErrorType = errorType };

    [Fact]
    public void Translate_WhenNull_Throws()
    {
        var act = () => _sut.Translate(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Translate_WhenStatusNotImplemented_ReturnsTerminalUnsupported()
    {
        var model = _sut.Translate(ServiceException(HttpStatusCode.NotImplemented, "Whatever"));

        model.Category.Should().Be(ErrorCategory.Unsupported);
        model.Classification.Should().Be(ErrorClassification.Terminal);
        model.IsRetryable.Should().BeFalse();
    }

    [Fact]
    public void Translate_WhenUnsupportedErrorCode_ReturnsUnsupported()
    {
        var model = _sut.Translate(ServiceException(HttpStatusCode.BadRequest, "UnsupportedOperation"));

        model.Category.Should().Be(ErrorCategory.Unsupported);
    }

    [Fact]
    public void Translate_WhenMessageMentionsNotImplemented_ReturnsUnsupported()
    {
        var model = _sut.Translate(ServiceException(HttpStatusCode.BadRequest, "GenericError", message: "API not yet implemented"));

        model.Category.Should().Be(ErrorCategory.Unsupported);
    }

    [Theory]
    [InlineData("This is not implemented in the backend")]
    [InlineData("That action is not supported here")]
    public void Translate_WhenMessageIndicatesUnsupported_ReturnsUnsupported(string message)
    {
        var model = _sut.Translate(ServiceException(HttpStatusCode.BadRequest, "GenericError", message: message));

        model.Category.Should().Be(ErrorCategory.Unsupported);
    }

    [Fact]
    public void Translate_WhenStatusTooManyRequests_ReturnsRetryableThrottling()
    {
        var model = _sut.Translate(ServiceException(HttpStatusCode.TooManyRequests, "GenericError"));

        model.Category.Should().Be(ErrorCategory.Throttling);
        model.Classification.Should().Be(ErrorClassification.Retryable);
    }

    [Fact]
    public void Translate_WhenThrottlingErrorCode_ReturnsThrottling()
    {
        var model = _sut.Translate(ServiceException(HttpStatusCode.BadRequest, "ThrottlingException"));

        model.Category.Should().Be(ErrorCategory.Throttling);
    }

    [Fact]
    public void Translate_WhenReceiverErrorType_ReturnsRetryableTransient()
    {
        var model = _sut.Translate(ServiceException(HttpStatusCode.BadRequest, "GenericError", ErrorType.Receiver));

        model.Category.Should().Be(ErrorCategory.Transient);
        model.Classification.Should().Be(ErrorClassification.Retryable);
    }

    [Fact]
    public void Translate_WhenServerError_ReturnsTransient()
    {
        var model = _sut.Translate(ServiceException(HttpStatusCode.ServiceUnavailable, "GenericError", ErrorType.Sender));

        model.Category.Should().Be(ErrorCategory.Transient);
    }

    [Fact]
    public void Translate_WhenStatusNotFound_ReturnsTerminalNotFound()
    {
        var model = _sut.Translate(ServiceException(HttpStatusCode.NotFound, "GenericError", ErrorType.Sender));

        model.Category.Should().Be(ErrorCategory.NotFound);
        model.Classification.Should().Be(ErrorClassification.Terminal);
    }

    [Fact]
    public void Translate_WhenNotFoundErrorCode_ReturnsNotFound()
    {
        var model = _sut.Translate(ServiceException(HttpStatusCode.BadRequest, "ResourceNotFoundException", ErrorType.Sender));

        model.Category.Should().Be(ErrorCategory.NotFound);
    }

    [Fact]
    public void Translate_WhenForbidden_ReturnsAccessDenied()
    {
        var model = _sut.Translate(ServiceException(HttpStatusCode.Forbidden, "GenericError", ErrorType.Sender));

        model.Category.Should().Be(ErrorCategory.AccessDenied);
    }

    [Fact]
    public void Translate_WhenUnauthorized_ReturnsAccessDenied()
    {
        var model = _sut.Translate(ServiceException(HttpStatusCode.Unauthorized, "GenericError", ErrorType.Sender));

        model.Category.Should().Be(ErrorCategory.AccessDenied);
    }

    [Fact]
    public void Translate_WhenAccessDeniedErrorCode_ReturnsAccessDenied()
    {
        var model = _sut.Translate(ServiceException((HttpStatusCode)418, "AccessDeniedException", ErrorType.Sender));

        model.Category.Should().Be(ErrorCategory.AccessDenied);
    }

    [Fact]
    public void Translate_WhenBadRequest_ReturnsValidation()
    {
        var model = _sut.Translate(ServiceException(HttpStatusCode.BadRequest, "GenericError", ErrorType.Sender));

        model.Category.Should().Be(ErrorCategory.Validation);
    }

    [Fact]
    public void Translate_WhenValidationErrorCode_ReturnsValidation()
    {
        var model = _sut.Translate(ServiceException(HttpStatusCode.UnprocessableEntity, "ValidationException", ErrorType.Sender));

        model.Category.Should().Be(ErrorCategory.Validation);
    }

    [Fact]
    public void Translate_WhenUnclassified_ReturnsTerminalUnknown()
    {
        var model = _sut.Translate(ServiceException((HttpStatusCode)418, "GenericError", ErrorType.Sender));

        model.Category.Should().Be(ErrorCategory.Unknown);
        model.Classification.Should().Be(ErrorClassification.Terminal);
    }

    [Fact]
    public void Translate_WhenErrorCodeMissing_UsesExceptionTypeNameAsCode()
    {
        var model = _sut.Translate(ServiceException(HttpStatusCode.BadRequest, errorCode: null, ErrorType.Sender));

        model.Code.Should().Be(nameof(AmazonServiceException));
    }

    [Fact]
    public void Translate_WhenClientException_ReturnsRetryableTransient()
    {
        var model = _sut.Translate(new AmazonClientException("no connection"));

        model.Code.Should().Be("ClientError");
        model.Category.Should().Be(ErrorCategory.Transient);
        model.Classification.Should().Be(ErrorClassification.Retryable);
    }

    [Fact]
    public void Translate_WhenHttpRequestException_ReturnsTransient()
    {
        var model = _sut.Translate(new HttpRequestException("network"));

        model.Code.Should().Be("HttpRequestError");
        model.Category.Should().Be(ErrorCategory.Transient);
    }

    [Fact]
    public void Translate_WhenTimeoutException_ReturnsTransient()
    {
        var model = _sut.Translate(new TimeoutException("timed out"));

        model.Code.Should().Be("Timeout");
        model.Category.Should().Be(ErrorCategory.Transient);
    }

    [Fact]
    public void Translate_WhenUnrecognisedException_ReturnsTerminalUnknown()
    {
        var model = _sut.Translate(new InvalidOperationException("weird"));

        model.Code.Should().Be(nameof(InvalidOperationException));
        model.Category.Should().Be(ErrorCategory.Unknown);
        model.Classification.Should().Be(ErrorClassification.Terminal);
    }
}
