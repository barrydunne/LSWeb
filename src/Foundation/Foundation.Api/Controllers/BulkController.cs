using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Commands.ExecuteBulkAction;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Api.Controllers;

/// <summary>
/// Provides endpoints for applying actions to multiple resources in a single request.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/bulk")]
public partial class BulkController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BulkController"/> class.
    /// </summary>
    /// <param name="sender">The sender used to dispatch commands.</param>
    /// <param name="logger">The logger.</param>
    public BulkController(ISender sender, ILogger<BulkController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Applies the specified action to a set of resources, returning a per-item result so that
    /// partial success is reported, and broadcasting progress to connected clients.
    /// </summary>
    /// <param name="actionName">The action to apply, taken from the route, for example <c>delete</c>.</param>
    /// <param name="request">The resources to apply the action to.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the aggregate outcome with per-item results.</returns>
    [HttpPost("{actionName}")]
    [ProducesResponseType(typeof(BulkActionResponse), StatusCodes.Status200OK)]
    public async Task<IResult> Execute(string actionName, [FromBody] BulkActionRequest request, CancellationToken cancellationToken)
    {
        LogHandling(actionName, request.ResourceIds?.Count ?? 0);
        var result = await _sender.Send(
            new ExecuteBulkActionCommand(actionName, request.ResourceIds ?? []),
            cancellationToken);
        LogHandled(result.IsSuccess);
        return result.Match(
            outcome => Results.Ok(new BulkActionResponse(
                outcome.OperationId,
                outcome.Action,
                outcome.TotalCount,
                outcome.SucceededCount,
                outcome.FailedCount,
                outcome.OverallState.ToString(),
                outcome.Items
                    .Select(item => new BulkActionItemResponse(item.ResourceId, item.Succeeded, item.Error))
                    .ToList())),
            error => error.AsHttpResult());
    }

    [LoggerMessage(LogLevel.Trace, "Handling bulk action '{Action}' for {Count} resource(s).")]
    private partial void LogHandling(string action, int count);

    [LoggerMessage(LogLevel.Trace, "Bulk action request handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
