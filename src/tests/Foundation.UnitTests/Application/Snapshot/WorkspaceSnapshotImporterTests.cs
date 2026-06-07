using Foundation.Application.Snapshot;
using Foundation.Domain.Snapshot;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Snapshot;

public class WorkspaceSnapshotImporterTests
{
    private readonly Lazy<WorkspaceSnapshotImporter> _sut = new(() =>
        new WorkspaceSnapshotImporter(NullLogger<WorkspaceSnapshotImporter>.Instance));

    [Fact]
    public async Task ImportAsync_WithValidSnapshot_ReturnsSuccessfulOutcome()
    {
        // Arrange
        var snapshot = new WorkspaceSnapshot(
            "snap-abc123",
            DateTime.UtcNow,
            new Dictionary<string, IReadOnlyList<SnapshotResourceData>>
            {
                { "lambda", new[] { new SnapshotResourceData("lambda", "Function", "func-1", "test-func", "{}") } }
            });

        // Act
        var result = await _sut.Value.ImportAsync(snapshot, TestContext.Current.CancellationToken);

        // Assert
        result.OperationType.Should().Be("Import");
        result.ResourceCount.Should().Be(1);
        result.SuccessCount.Should().Be(1);
        result.FailureCount.Should().Be(0);
        result.Failures.Should().BeEmpty();
    }

    [Fact]
    public async Task ImportAsync_WithEmptySnapshot_ReturnsOutcomeWithZeroResources()
    {
        // Arrange
        var snapshot = new WorkspaceSnapshot(
            "snap-empty",
            DateTime.UtcNow,
            new Dictionary<string, IReadOnlyList<SnapshotResourceData>>());

        // Act
        var result = await _sut.Value.ImportAsync(snapshot, TestContext.Current.CancellationToken);

        // Assert
        result.ResourceCount.Should().Be(0);
        result.SuccessCount.Should().Be(0);
        result.FailureCount.Should().Be(0);
    }

    [Fact]
    public async Task ImportAsync_WithMultipleServices_GroupsResources()
    {
        // Arrange
        var snapshot = new WorkspaceSnapshot(
            "snap-multi",
            DateTime.UtcNow,
            new Dictionary<string, IReadOnlyList<SnapshotResourceData>>
            {
                { "lambda", new[] { new SnapshotResourceData("lambda", "Function", "func-1", "test-func", "{}") } },
                { "sqs", new[] { new SnapshotResourceData("sqs", "Queue", "queue-1", "seed-queue", "{}") } },
                { "s3", new[] { new SnapshotResourceData("s3", "Bucket", "bucket-1", "seed-bucket", "{}") } }
            });

        // Act
        var result = await _sut.Value.ImportAsync(snapshot, TestContext.Current.CancellationToken);

        // Assert
        result.ResourceCount.Should().Be(3);
        result.SuccessCount.Should().Be(3);
    }
}
