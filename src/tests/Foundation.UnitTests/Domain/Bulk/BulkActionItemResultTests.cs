using Foundation.Domain.Bulk;

namespace Foundation.UnitTests.Domain.Bulk;

public class BulkActionItemResultTests
{
    [Fact]
    public void Constructor_ExposesAllProperties()
    {
        var result = new BulkActionItemResult("s3://bucket", false, "Resource id is required.");

        result.ResourceId.Should().Be("s3://bucket");
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Resource id is required.");
    }
}
