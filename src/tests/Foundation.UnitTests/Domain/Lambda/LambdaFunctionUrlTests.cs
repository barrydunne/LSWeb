using Foundation.Domain.Lambda;

namespace Foundation.UnitTests.Domain.Lambda;

public class LambdaFunctionUrlTests
{
    [Fact]
    public void Properties_ExposeConstructorValues()
    {
        // Act
        var url = new LambdaFunctionUrl(
            "https://abc.lambda-url.eu-west-1.on.aws/",
            "NONE",
            "2026-01-02T03:04:05Z",
            "2026-01-03T03:04:05Z");

        // Assert
        url.FunctionUrl.Should().Be("https://abc.lambda-url.eu-west-1.on.aws/");
        url.AuthType.Should().Be("NONE");
        url.CreationTime.Should().Be("2026-01-02T03:04:05Z");
        url.LastModifiedTime.Should().Be("2026-01-03T03:04:05Z");
    }
}
