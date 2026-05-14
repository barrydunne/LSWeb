using Foundation.Domain.Search;
using Foundation.Infrastructure.Search;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Infrastructure.Search;

public class SearchIndexBuilderTests
{
    private static readonly DateTimeOffset _now = new(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);

    private static SearchIndexBuilder CreateSut(params IResourceSource[] sources)
        => new(sources, new FixedTimeProvider(_now), NullLogger<SearchIndexBuilder>.Instance);

    [Fact]
    public async Task BuildAsync_WhenNoSources_ReturnsEmptySnapshotStampedWithCurrentTime()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var snapshot = await sut.BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        snapshot.Entries.Should().BeEmpty();
        snapshot.Count.Should().Be(0);
        snapshot.BuiltAt.Should().Be(_now);
    }

    [Fact]
    public async Task BuildAsync_WithMultipleSources_CollectsEveryResource()
    {
        // Arrange
        var sut = CreateSut(
            new StubSource("sqs", new SearchEntry("sqs", "orders", "orders (queue)", "/services/sqs/orders")),
            new StubSource("s3", new SearchEntry("s3", "assets", "assets (bucket)", "/services/s3/assets")));

        // Act
        var snapshot = await sut.BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        snapshot.Entries.Should().HaveCount(2);
        snapshot.Entries.Select(_ => _.ResourceId).Should().Contain(["orders", "assets"]);
    }

    [Fact]
    public async Task BuildAsync_OrdersEntriesByServiceKeyThenResourceId()
    {
        // Arrange
        var sut = CreateSut(
            new StubSource(
                "sqs",
                new SearchEntry("sqs", "zeta", "zeta (queue)", "/services/sqs/zeta"),
                new SearchEntry("sqs", "alpha", "alpha (queue)", "/services/sqs/alpha")),
            new StubSource("s3", new SearchEntry("s3", "bucket", "bucket (bucket)", "/services/s3/bucket")));

        // Act
        var snapshot = await sut.BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        snapshot.Entries.Select(_ => _.ResourceId).Should().Equal("bucket", "alpha", "zeta");
    }

    [Fact]
    public async Task BuildAsync_WhenOneSourceThrows_IsolatesItAndKeepsOtherEntries()
    {
        // Arrange
        var sut = CreateSut(
            new ThrowingSource("lambda", new InvalidOperationException("boom")),
            new StubSource("s3", new SearchEntry("s3", "assets", "assets (bucket)", "/services/s3/assets")));

        // Act
        var snapshot = await sut.BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        snapshot.Entries.Should().ContainSingle(_ => _.ResourceId == "assets");
    }

    [Fact]
    public async Task BuildAsync_WhenSourceCancels_PropagatesCancellation()
    {
        // Arrange
        var sut = CreateSut(new ThrowingSource("sqs", new OperationCanceledException()));

        // Act
        var act = async () => await sut.BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    private sealed class StubSource(string serviceKey, params SearchEntry[] entries) : IResourceSource
    {
        public string ServiceKey { get; } = serviceKey;

        public Task<IReadOnlyList<SearchEntry>> ListAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<SearchEntry>>(entries);
    }

    private sealed class ThrowingSource(string serviceKey, Exception exception) : IResourceSource
    {
        public string ServiceKey { get; } = serviceKey;

        public Task<IReadOnlyList<SearchEntry>> ListAsync(CancellationToken cancellationToken)
            => throw exception;
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        private readonly DateTimeOffset _now = now;

        public override DateTimeOffset GetUtcNow() => _now;
    }
}
