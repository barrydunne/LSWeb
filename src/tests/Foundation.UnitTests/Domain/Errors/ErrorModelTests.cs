using Foundation.Domain.Errors;

namespace Foundation.UnitTests.Domain.Errors;

public class ErrorModelTests
{
    [Fact]
    public void IsRetryable_WhenRetryable_ReturnsTrue()
    {
        var model = new ErrorModel("Throttling", "busy", ErrorCategory.Throttling, ErrorClassification.Retryable);

        model.IsRetryable.Should().BeTrue();
    }

    [Fact]
    public void IsRetryable_WhenTerminal_ReturnsFalse()
    {
        var model = new ErrorModel("AccessDenied", "denied", ErrorCategory.AccessDenied, ErrorClassification.Terminal);

        model.IsRetryable.Should().BeFalse();
    }

    [Fact]
    public void Constructor_ExposesAllProperties()
    {
        var model = new ErrorModel("Code", "Message", ErrorCategory.Validation, ErrorClassification.Terminal);

        model.Code.Should().Be("Code");
        model.Message.Should().Be("Message");
        model.Category.Should().Be(ErrorCategory.Validation);
        model.Classification.Should().Be(ErrorClassification.Terminal);
    }
}
