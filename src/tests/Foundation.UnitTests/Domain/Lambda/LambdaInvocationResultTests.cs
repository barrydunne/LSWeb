using Foundation.Domain.Lambda;

namespace Foundation.UnitTests.Domain.Lambda;

public class LambdaInvocationResultTests
{
    [Fact]
    public void Properties_ExposeConstructorValues()
    {
        // Act
        var result = new LambdaInvocationResult(200, "{\"ok\":true}", "log tail", "Unhandled", 42);

        // Assert
        result.StatusCode.Should().Be(200);
        result.Payload.Should().Be("{\"ok\":true}");
        result.LogTail.Should().Be("log tail");
        result.FunctionError.Should().Be("Unhandled");
        result.DurationMs.Should().Be(42);
    }

    [Fact]
    public void HasFunctionError_WhenErrorPresent_IsTrue()
    {
        // Act
        var result = new LambdaInvocationResult(200, "{}", string.Empty, "Unhandled", 1);

        // Assert
        result.HasFunctionError.Should().BeTrue();
    }

    [Fact]
    public void HasFunctionError_WhenErrorEmpty_IsFalse()
    {
        // Act
        var result = new LambdaInvocationResult(200, "{}", string.Empty, string.Empty, 1);

        // Assert
        result.HasFunctionError.Should().BeFalse();
    }
}
