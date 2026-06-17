using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Commands.ImportWorkspaceSnapshot;
using Foundation.Application.Queries.ExportWorkspaceSnapshot;
using Foundation.Domain.Snapshot;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Api.Controllers;

/// <summary>
/// Provides endpoints for exporting and importing workspace snapshots, allowing users to capture the
/// current state of their resources and restore them later.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/snapshot")]
public partial class SnapshotController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<SnapshotController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SnapshotController"/> class.
    /// </summary>
    /// <param name="sender">The sender used to dispatch queries and commands.</param>
    /// <param name="logger">The logger.</param>
    public SnapshotController(ISender sender, ILogger<SnapshotController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Exports the current workspace state as a downloadable snapshot file. The response is the full
    /// snapshot document (the same shape the import endpoint accepts) so an exported snapshot can be
    /// re-imported without transformation.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the full snapshot document as JSON for download.</returns>
    [HttpGet("export")]
    [ProducesResponseType(typeof(WorkspaceSnapshot), StatusCodes.Status200OK)]
    public async Task<IResult> Export(CancellationToken cancellationToken)
    {
        LogHandlingExport();
        var result = await _sender.Send(new ExportWorkspaceSnapshotQuery(), cancellationToken);
        LogExportHandled(result.IsSuccess);
        return result.Match(
            snapshot => Results.Ok(snapshot),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Imports a previously exported workspace snapshot, recreating its resources.
    /// </summary>
    /// <param name="snapshot">The snapshot data to import.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the per-resource import outcome.</returns>
    [HttpPost("import")]
    [ProducesResponseType(typeof(SnapshotImportResponse), StatusCodes.Status200OK)]
    public async Task<IResult> Import([FromBody] WorkspaceSnapshot snapshot, CancellationToken cancellationToken)
    {
        LogHandlingImport(snapshot.Id);
        var result = await _sender.Send(new ImportWorkspaceSnapshotCommand(snapshot), cancellationToken);
        LogImportHandled(result.IsSuccess);
        return result.Match(
            outcome => Results.Ok(new SnapshotImportResponse(
                outcome.OperationId,
                outcome.OperationType,
                outcome.CompletedAt,
                outcome.ResourceCount,
                outcome.SuccessCount,
                outcome.FailureCount,
                outcome.Failures
                    .Select(f => new SnapshotFailureResponse(f.Service, f.ResourceId, f.Error))
                    .ToList())),
            error => error.AsHttpResult());
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Handling snapshot export")]
    private partial void LogHandlingExport();

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Snapshot export handled, success: {Success}")]
    private partial void LogExportHandled(bool success);

    [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "Handling snapshot import for snapshot {SnapshotId}")]
    private partial void LogHandlingImport(string snapshotId);

    [LoggerMessage(EventId = 4, Level = LogLevel.Debug, Message = "Snapshot import handled, success: {Success}")]
    private partial void LogImportHandled(bool success);
}
