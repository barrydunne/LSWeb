using Foundation.Infrastructure.S3;

namespace Foundation.UnitTests.Infrastructure.S3;

public class PresignedUrlRewriterTests
{
    [Fact]
    public void Rewrite_WhenConfiguredPublicEndpoint_SwapsSchemeAndAuthorityPreservingPathAndQuery()
    {
        // Arrange
        var sut = new PresignedUrlRewriter("http://localstack:4566", "http://localhost:4566");

        // Act
        var result = sut.Rewrite("https://localstack:4566/qa-bucket/readme.txt?X-Amz-Signature=abc%2Fdef");

        // Assert
        result.Should().Be("http://localhost:4566/qa-bucket/readme.txt?X-Amz-Signature=abc%2Fdef");
    }

    [Fact]
    public void Rewrite_WhenNoPublicEndpointConfigured_DerivesLocalhostFromInternalPort()
    {
        // Arrange
        var sut = new PresignedUrlRewriter("http://localstack:4566", null);

        // Act
        var result = sut.Rewrite("https://localstack:4566/bucket/key?sig=1");

        // Assert
        result.Should().Be("http://localhost:4566/bucket/key?sig=1");
    }

    [Fact]
    public void Rewrite_WhenInternalEndpointMissing_DefaultsToLocalhost4566()
    {
        // Arrange
        var sut = new PresignedUrlRewriter(null, null);

        // Act
        var result = sut.Rewrite("https://localstack:4566/bucket/key");

        // Assert
        result.Should().Be("http://localhost:4566/bucket/key");
    }

    [Fact]
    public void Rewrite_WhenInternalEndpointUsesADefaultPort_FallsBackTo4566()
    {
        // Arrange
        var sut = new PresignedUrlRewriter("https://localstack", null);

        // Act
        var result = sut.Rewrite("https://localstack/bucket/key");

        // Assert
        result.Should().Be("http://localhost:4566/bucket/key");
    }

    [Fact]
    public void Rewrite_WhenAlreadyTargetingThePublicAuthority_ReturnsUrlUnchanged()
    {
        // Arrange
        var sut = new PresignedUrlRewriter("http://localhost:4566", "http://localhost:4566");

        // Act
        var url = "http://localhost:4566/bucket/key?sig=1";
        var result = sut.Rewrite(url);

        // Assert
        result.Should().BeSameAs(url);
    }

    [Fact]
    public void Rewrite_WhenUrlIsNotAbsolute_ReturnsItUnchanged()
    {
        // Arrange
        var sut = new PresignedUrlRewriter("http://localstack:4566", "http://localhost:4566");

        // Act
        var result = sut.Rewrite("/relative/path");

        // Assert
        result.Should().Be("/relative/path");
    }

    [Fact]
    public void Rewrite_WhenUrlSchemeIsNotHttp_ReturnsItUnchanged()
    {
        // Arrange
        var sut = new PresignedUrlRewriter("http://localstack:4566", "http://localhost:4566");

        // Act
        var result = sut.Rewrite("ftp://localstack:4566/bucket/key");

        // Assert
        result.Should().Be("ftp://localstack:4566/bucket/key");
    }

    [Fact]
    public void Rewrite_WhenConfiguredPublicEndpointIsNotAbsolute_DerivesFromInternalPort()
    {
        // Arrange
        var sut = new PresignedUrlRewriter("http://localstack:9000", "not-a-url");

        // Act
        var result = sut.Rewrite("https://localstack:9000/bucket/key");

        // Assert
        result.Should().Be("http://localhost:9000/bucket/key");
    }
}
