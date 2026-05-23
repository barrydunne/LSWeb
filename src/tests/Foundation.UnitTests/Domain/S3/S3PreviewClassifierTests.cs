using Foundation.Domain.S3;

namespace Foundation.UnitTests.Domain.S3;

public class S3PreviewClassifierTests
{
    [Theory]
    [InlineData("image/png", "logo.png")]
    [InlineData("image/jpeg", "photo")]
    [InlineData("application/octet-stream", "photo.jpg")]
    [InlineData("application/octet-stream", "icon.svg")]
    public void Classify_WhenImage_ReturnsImage(string contentType, string key)
        => S3PreviewClassifier.Classify(contentType, key).Should().Be(S3PreviewKind.Image);

    [Theory]
    [InlineData("application/json", "config")]
    [InlineData("application/octet-stream", "config.json")]
    public void Classify_WhenJson_ReturnsJson(string contentType, string key)
        => S3PreviewClassifier.Classify(contentType, key).Should().Be(S3PreviewKind.Json);

    [Theory]
    [InlineData("text/plain", "notes")]
    [InlineData("application/xml", "feed")]
    [InlineData("application/octet-stream", "script.sh")]
    [InlineData("application/octet-stream", "notes.md")]
    public void Classify_WhenText_ReturnsText(string contentType, string key)
        => S3PreviewClassifier.Classify(contentType, key).Should().Be(S3PreviewKind.Text);

    [Theory]
    [InlineData("application/octet-stream", "archive.bin")]
    [InlineData("application/zip", "data")]
    [InlineData("application/pdf", "report.pdf")]
    public void Classify_WhenUnknown_ReturnsBinary(string contentType, string key)
        => S3PreviewClassifier.Classify(contentType, key).Should().Be(S3PreviewKind.Binary);

    [Fact]
    public void Classify_WhenContentTypeHasMixedCaseAndWhitespace_NormalizesBeforeMatching()
        => S3PreviewClassifier.Classify("  TEXT/Plain  ", "data").Should().Be(S3PreviewKind.Text);
}
