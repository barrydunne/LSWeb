using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Commands.ImportWorkspaceSnapshot;
using Foundation.Application.Snapshot;
using Foundation.Domain.Snapshot;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Application.Commands.ImportWorkspaceSnapshot;

public class ImportWorkspaceSnapshotCommandHandlerTests
{
    private readonly Lazy<ImportWorkspaceSnapshotCommandHandler> _sut;
    private readonly IWorkspaceSnapshotImporter _importer;

    public ImportWorkspaceSnapshotCommandHandlerTests()
    {
        _importer = Substitute.For<IWorkspaceSnapshotImporter>();
        _sut = new(() => new ImportWorkspaceSnapshotCommandHandler(_importer, NullLogger<ImportWorkspaceSnapshotCommandHandler>.Instance));
    }

    [Fact]
    public async Task Handle_WithValidSnapshot_ReturnsSuccessWithOutcome()
    {
        // Arrange
        var snapshot = new WorkspaceSnapshot(
            "snap-abc123",
            DateTime.UtcNow,
            new Dictionary<string, IReadOnlyList<SnapshotResourceData>>
            {
                { "lambda", new[] { new SnapshotResourceData("lambda", "Function", "func-1", "test-func", "{}") } }
            });

        var outcome = new SnapshotOutcome(
            "imp-xyz789",
            "Import",
            DateTime.UtcNow,
            1,
            1,
            0,
            new List<SnapshotFailureDetail>());

        _importer
            .ImportAsync(snapshot, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(outcome));

        var command = new ImportWorkspaceSnapshotCommand(snapshot);

        // Act
        var result = await _sut.Value.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.SuccessCount.Should().Be(1);
        await _importer.Received(1).ImportAsync(snapshot, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenImporterThrows_ReturnsFailure()
    {
        // Arrange
        var snapshot = new WorkspaceSnapshot(
            "snap-abc123",
            DateTime.UtcNow,
            new Dictionary<string, IReadOnlyList<SnapshotResourceData>>());

        _importer
            .ImportAsync(Arg.Any<WorkspaceSnapshot>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<SnapshotOutcome>(new InvalidOperationException("Import failed")));

        var command = new ImportWorkspaceSnapshotCommand(snapshot);

        // Act & Assert
        var result = await _sut.Value.Handle(command, TestContext.Current.CancellationToken);
        result.IsSuccess.Should().BeFalse();
    }
}
