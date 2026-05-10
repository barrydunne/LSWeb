using Foundation.Domain.Capabilities;

namespace Foundation.UnitTests.Domain.Capabilities;

public class CapabilityMapTests
{
    [Fact]
    public void Empty_HasNoEntries()
        => CapabilityMap.Empty.Entries.Should().BeEmpty();

    [Fact]
    public void Find_WhenKeyExists_ReturnsEntry()
    {
        var map = new CapabilityMap([new CapabilityEntry("s3", CapabilityStatus.Supported, null)]);

        var entry = map.Find("s3");

        entry.Should().NotBeNull();
        entry!.Status.Should().Be(CapabilityStatus.Supported);
    }

    [Fact]
    public void Find_WhenKeyMissing_ReturnsNull()
    {
        var map = new CapabilityMap([new CapabilityEntry("s3", CapabilityStatus.Supported, null)]);

        map.Find("sqs").Should().BeNull();
    }

    [Fact]
    public void IsSupported_WhenServiceSupported_ReturnsTrue()
    {
        var map = new CapabilityMap([new CapabilityEntry("s3", CapabilityStatus.Supported, null)]);

        map.IsSupported("s3").Should().BeTrue();
    }

    [Fact]
    public void IsSupported_WhenServiceUnsupported_ReturnsFalse()
    {
        var map = new CapabilityMap([new CapabilityEntry("s3", CapabilityStatus.Unsupported, "nope")]);

        map.IsSupported("s3").Should().BeFalse();
    }

    [Fact]
    public void IsSupported_WhenServiceMissing_ReturnsFalse()
    {
        var map = new CapabilityMap([new CapabilityEntry("s3", CapabilityStatus.Supported, null)]);

        map.IsSupported("sqs").Should().BeFalse();
    }

    [Fact]
    public void Constructor_ExposesEntries()
    {
        var entry = new CapabilityEntry("s3", CapabilityStatus.Unknown, "detail");

        var map = new CapabilityMap([entry]);

        map.Entries.Should().ContainSingle().Which.Should().Be(entry);
    }
}
