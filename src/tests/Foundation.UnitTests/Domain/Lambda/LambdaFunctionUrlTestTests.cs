using Foundation.Domain.Lambda;

namespace Foundation.UnitTests.Domain.Lambda;

public class LambdaFunctionUrlTestTests
{
    [Fact]
    public void Properties_ExposeConstructorValues()
    {
        // Act
        var test = new LambdaFunctionUrlTest(200, "{\"ok\":true}");

        // Assert
        test.StatusCode.Should().Be(200);
        test.Body.Should().Be("{\"ok\":true}");
    }
}
