using Foundation.Domain.Search;

namespace Foundation.UnitTests.Domain.Search;

public class SearchEntryTests
{
    [Fact]
    public void Constructor_ExposesAllProperties()
    {
        var entry = new SearchEntry("sqs", "orders", "orders (queue)", "/services/sqs/orders");

        entry.ServiceKey.Should().Be("sqs");
        entry.ResourceId.Should().Be("orders");
        entry.DisplayName.Should().Be("orders (queue)");
        entry.Route.Should().Be("/services/sqs/orders");
    }

    [Fact]
    public void Equality_TreatsEntriesWithSameValuesAsEqual()
    {
        var first = new SearchEntry("sqs", "orders", "orders (queue)", "/services/sqs/orders");
        var second = new SearchEntry("sqs", "orders", "orders (queue)", "/services/sqs/orders");

        first.Should().Be(second);
    }

    [Fact]
    public void Equality_TreatsEntriesWithDifferentValuesAsNotEqual()
    {
        var first = new SearchEntry("sqs", "orders", "orders (queue)", "/services/sqs/orders");
        var second = new SearchEntry("s3", "assets", "assets (bucket)", "/services/s3/assets");

        first.Should().NotBe(second);
    }
}
