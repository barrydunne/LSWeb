using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Queries.ExportWorkspaceSnapshot;
using Foundation.Application.Snapshot;
using Foundation.Domain.Snapshot;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Queries.ExportWorkspaceSnapshot;

public class ExportWorkspaceSnapshotQueryHandlerTests
{
    private readonly Lazy<ExportWorkspaceSnapshotQueryHandler> _sut;
    private readonly IWorkspaceSnapshotExporter _exporter;

    public ExportWorkspaceSnapshotQueryHandlerTests()
    {
        _exporter = Substitute.For<IWorkspaceSnapshotExporter>();
        _sut = new(() => new ExportWorkspaceSnapshotQueryHandler(_exporter, NullLogger<ExportWorkspaceSnapshotQueryHandler>.Instance));
    }

    [Fact]
    public async Task Handle_WithValidRequest_ReturnsSuccessWithSnapshot()
    {
        // Arrange
        var snapshot = new WorkspaceSnapshot(
            "snap-abc123",
            DateTime.UtcNow,
            new Dictionary<string, IReadOnlyList<SnapshotResourceData>>());

        _exporter
            .ExportAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(snapshot));

        var query = new ExportWorkspaceSnapshotQuery();

        // Act
        var result = await _sut.Value.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(snapshot);
        await _exporter.Received(1).ExportAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExporterThrows_ReturnsFailure()
    {
        // Arrange
        _exporter
            .ExportAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<WorkspaceSnapshot>(new InvalidOperationException("Export failed")));

        var query = new ExportWorkspaceSnapshotQuery();

        // Act & Assert
        await _sut.Value.Invoking(h => h.Handle(query, TestContext.Current.CancellationToken)).Should().NotThrowAsync();
        var result = await _sut.Value.Handle(query, TestContext.Current.CancellationToken);
        result.IsSuccess.Should().BeFalse();
    }
}
