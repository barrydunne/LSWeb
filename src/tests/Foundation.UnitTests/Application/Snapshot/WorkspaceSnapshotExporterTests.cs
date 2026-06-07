using Foundation.Application.Snapshot;
using Foundation.Domain.Search;

namespace Foundation.UnitTests.Application.Snapshot;

public class WorkspaceSnapshotExporterTests
{
    private readonly Lazy<WorkspaceSnapshotExporter> _sut;
    private readonly ISearchIndexStore _searchIndex;

    public WorkspaceSnapshotExporterTests()
    {
        _searchIndex = Substitute.For<ISearchIndexStore>();
        _sut = new(() => new WorkspaceSnapshotExporter(_searchIndex, Microsoft.Extensions.Logging.Abstractions.NullLogger<WorkspaceSnapshotExporter>.Instance));
    }

    [Fact]
    public async Task ExportAsync_WithEmptyIndex_ReturnsSnapshotWithNoResources()
    {
        // Arrange
        _searchIndex.GetCurrent().Returns((SearchIndexState?)null);

        // Act
        var result = await _sut.Value.ExportAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Resources.Should().BeEmpty();
        result.ExportedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task ExportAsync_WithResources_ReturnsSnapshotGroupedByService()
    {
        // Arrange
        var entries = new[]
        {
            new SearchEntry("lambda", "func-1", "test-func", "/lambda/func-1"),
            new SearchEntry("lambda", "func-2", "another-func", "/lambda/func-2"),
            new SearchEntry("sqs", "queue-1", "seed-queue", "/sqs/queue-1")
        };

        var index = new SearchIndexState(entries, DateTimeOffset.UtcNow);
        _searchIndex.GetCurrent().Returns(index);

        // Act
        var result = await _sut.Value.ExportAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Resources.Should().HaveCount(2);
        result.Resources.Should().ContainKey("lambda");
        result.Resources.Should().ContainKey("sqs");
        result.Resources["lambda"].Should().HaveCount(2);
        result.Resources["sqs"].Should().HaveCount(1);
    }
}
