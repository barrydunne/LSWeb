using Foundation.Domain.Search;

namespace Foundation.UnitTests.Domain.Search;

public class SearchIndexStateTests
{
    [Fact]
    public void Empty_HasNoEntriesAndMinBuiltAt()
    {
        var state = SearchIndexState.Empty;

        state.Entries.Should().BeEmpty();
        state.Count.Should().Be(0);
        state.BuiltAt.Should().Be(DateTimeOffset.MinValue);
    }

    [Fact]
    public void Constructor_ExposesEntriesBuiltAtAndCount()
    {
        var builtAt = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var entries = new[]
        {
            new SearchEntry("sqs", "orders", "orders (queue)", "/services/sqs/orders"),
            new SearchEntry("s3", "assets", "assets (bucket)", "/services/s3/assets"),
        };

        var state = new SearchIndexState(entries, builtAt);

        state.Entries.Should().BeSameAs(entries);
        state.BuiltAt.Should().Be(builtAt);
        state.Count.Should().Be(2);
    }
}
