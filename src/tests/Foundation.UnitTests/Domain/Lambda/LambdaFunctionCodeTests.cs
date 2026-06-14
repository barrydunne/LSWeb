using Foundation.Domain.Lambda;

namespace Foundation.UnitTests.Domain.Lambda;

public class LambdaFunctionCodeTests
{
    [Fact]
    public void Properties_ExposeConstructorValues()
    {
        // Act
        var code = new LambdaFunctionCode(
            "process-orders",
            "dotnet8",
            "Orders::Handler",
            "Zip",
            2048,
            "abc123=",
            "S3",
            "https://localstack/download.zip",
            "000000000000.dkr.ecr.eu-west-1.amazonaws.com/app:latest");

        // Assert
        code.FunctionName.Should().Be("process-orders");
        code.Runtime.Should().Be("dotnet8");
        code.Handler.Should().Be("Orders::Handler");
        code.PackageType.Should().Be("Zip");
        code.CodeSize.Should().Be(2048);
        code.CodeSha256.Should().Be("abc123=");
        code.RepositoryType.Should().Be("S3");
        code.Location.Should().Be("https://localstack/download.zip");
        code.ImageUri.Should().Be("000000000000.dkr.ecr.eu-west-1.amazonaws.com/app:latest");
    }
}
