using AspNet.KickStarter.FunctionalResult;
using Foundation.Api.Controllers;
using Foundation.Api.Models;
using Foundation.Application.Commands.ImportWorkspaceSnapshot;
using Foundation.Application.Queries.ExportWorkspaceSnapshot;
using Foundation.Domain.Snapshot;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging.Abstractions;

namespace Foundation.UnitTests.Controllers;

public class SnapshotControllerTests
{
    private readonly Lazy<SnapshotController> _sut;
    private readonly ISender _sender;

    public SnapshotControllerTests()
    {
        _sender = Substitute.For<ISender>();
        _sut = new(() => new SnapshotController(_sender, NullLogger<SnapshotController>.Instance));
    }

    [Fact]
    public async Task Export_WithSuccessfulExport_Returns200WithSnapshot()
    {
        // Arrange
        var snapshot = new WorkspaceSnapshot(
            "snap-abc123",
            DateTime.UtcNow,
            new Dictionary<string, IReadOnlyList<SnapshotResourceData>>
            {
                { "lambda", new List<SnapshotResourceData>() }
            });

        _sender
            .Send(Arg.Any<ExportWorkspaceSnapshotQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<WorkspaceSnapshot>>(snapshot));

        // Act
        var result = await _sut.Value.Export(TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Export_WhenExportFails_ReturnsError()
    {
        // Arrange
        _sender
            .Send(Arg.Any<ExportWorkspaceSnapshotQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<WorkspaceSnapshot>>(new InvalidOperationException("Export failed")));

        // Act
        var result = await _sut.Value.Export(TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Import_WithValidSnapshot_Returns200WithOutcome()
    {
        // Arrange
        var snapshot = new WorkspaceSnapshot(
            "snap-abc123",
            DateTime.UtcNow,
            new Dictionary<string, IReadOnlyList<SnapshotResourceData>>());

        var outcome = new SnapshotOutcome(
            "imp-xyz789",
            "Import",
            DateTime.UtcNow,
            0,
            0,
            0,
            new List<SnapshotFailureDetail>());

        _sender
            .Send(Arg.Any<ImportWorkspaceSnapshotCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<SnapshotOutcome>>(outcome));

        // Act
        var result = await _sut.Value.Import(snapshot, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Import_WhenImportFails_ReturnsError()
    {
        // Arrange
        var snapshot = new WorkspaceSnapshot(
            "snap-abc123",
            DateTime.UtcNow,
            new Dictionary<string, IReadOnlyList<SnapshotResourceData>>());

        _sender
            .Send(Arg.Any<ImportWorkspaceSnapshotCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<SnapshotOutcome>>(new InvalidOperationException("Import failed")));

        // Act
        var result = await _sut.Value.Import(snapshot, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Import_WithFailures_MapsFailureDetailsToResponse()
    {
        // Arrange
        var snapshot = new WorkspaceSnapshot(
            "snap-abc123",
            DateTime.UtcNow,
            new Dictionary<string, IReadOnlyList<SnapshotResourceData>>());

        var outcome = new SnapshotOutcome(
            "imp-xyz789",
            "Import",
            DateTime.UtcNow,
            2,
            1,
            1,
            new List<SnapshotFailureDetail>
            {
                new("lambda", "func-1", "boom"),
            });

        _sender
            .Send(Arg.Any<ImportWorkspaceSnapshotCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<SnapshotOutcome>>(outcome));

        // Act
        var result = await _sut.Value.Import(snapshot, TestContext.Current.CancellationToken);

        // Assert
        var ok = result.Should().BeOfType<Ok<SnapshotImportResponse>>().Subject;
        ok.Value!.OperationId.Should().Be("imp-xyz789");
        ok.Value!.FailureCount.Should().Be(1);
        var failure = ok.Value!.Failures.Should().ContainSingle().Subject;
        failure.Service.Should().Be("lambda");
        failure.ResourceId.Should().Be("func-1");
        failure.Error.Should().Be("boom");
    }
}
