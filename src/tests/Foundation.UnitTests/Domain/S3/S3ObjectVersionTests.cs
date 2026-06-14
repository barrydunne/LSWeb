using Foundation.Domain.S3;

namespace Foundation.UnitTests.Domain.S3;

public class S3ObjectVersionTests
{
    [Fact]
    public void Properties_ExposeConstructorValues()
    {
        // Act
        var version = new S3ObjectVersion("report.pdf", "v2", true, false, 1024, "2026-01-02T03:04:05Z");

        // Assert
        version.Key.Should().Be("report.pdf");
        version.VersionId.Should().Be("v2");
        version.IsLatest.Should().BeTrue();
        version.IsDeleteMarker.Should().BeFalse();
        version.Size.Should().Be(1024);
        version.LastModified.Should().Be("2026-01-02T03:04:05Z");
    }
}
