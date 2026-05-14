using Foundation.Domain.Search;
using Foundation.Infrastructure.Search;

namespace Foundation.UnitTests.Infrastructure.Search;

public class IndexStoreTests
{
    [Fact]
    public void GetCurrent_WhenNotReplaced_ReturnsEmptySnapshot()
    {
        var sut = new IndexStore();

        sut.GetCurrent().Should().BeSameAs(SearchIndexState.Empty);
    }

    [Fact]
    public void GetCurrent_AfterReplace_ReturnsTheNewSnapshot()
    {
        var sut = new IndexStore();
        var updated = new SearchIndexState(
            [new SearchEntry("sqs", "orders", "orders (queue)", "/services/sqs/orders")],
            DateTimeOffset.UnixEpoch);

        sut.Replace(updated);

        sut.GetCurrent().Should().BeSameAs(updated);
    }

    [Fact]
    public void Replace_OverwritesAnEarlierSnapshot()
    {
        var sut = new IndexStore();
        var first = new SearchIndexState([], DateTimeOffset.UnixEpoch);
        var second = new SearchIndexState(
            [new SearchEntry("s3", "assets", "assets (bucket)", "/services/s3/assets")],
            DateTimeOffset.UnixEpoch.AddMinutes(1));

        sut.Replace(first);
        sut.Replace(second);

        sut.GetCurrent().Should().BeSameAs(second);
    }
}
