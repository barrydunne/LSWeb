using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Queries.GetConnectivity;
using Foundation.Application.Queries.GetLiveness;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Api.Controllers;

/// <summary>
/// Provides system-level operational endpoints such as liveness checks.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/system")]
public partial class SystemController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemController"/> class.
    /// </summary>
    /// <param name="sender">The sender used to dispatch queries.</param>
    /// <param name="logger">The logger.</param>
    public SystemController(ISender sender, ILogger<SystemController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Reports whether the service is alive and able to serve requests.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the liveness status when the service is healthy.</returns>
    [HttpGet("health")]
    [ProducesResponseType(typeof(GetLivenessQueryResult), StatusCodes.Status200OK)]
    public async Task<IResult> Health(CancellationToken cancellationToken)
    {
        LogHandlingLiveness();
        var result = await _sender.Send(new GetLivenessQuery(), cancellationToken);
        LogLivenessHandled(result.IsSuccess);
        return result.Match(
            status => Results.Ok(status),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Reports whether the configured AWS backend is reachable, along with the resolved endpoint and region.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the connectivity status with credentials masked.</returns>
    [HttpGet("connectivity")]
    [ProducesResponseType(typeof(ConnectivityResponse), StatusCodes.Status200OK)]
    public async Task<IResult> Connectivity(CancellationToken cancellationToken)
    {
        LogHandlingConnectivity();
        var result = await _sender.Send(new GetConnectivityQuery(), cancellationToken);
        LogConnectivityHandled(result.IsSuccess);
        return result.Match(
            connectivity => Results.Ok(new ConnectivityResponse(
                connectivity.Connection.Status.ToString(),
                connectivity.Connection.Endpoint,
                connectivity.Connection.Region,
                connectivity.Connection.Error)),
            error => error.AsHttpResult());
    }

    [LoggerMessage(LogLevel.Trace, "Handling liveness request.")]
    private partial void LogHandlingLiveness();

    [LoggerMessage(LogLevel.Trace, "Liveness request handled. Success: {Success}")]
    private partial void LogLivenessHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling connectivity request.")]
    private partial void LogHandlingConnectivity();

    [LoggerMessage(LogLevel.Trace, "Connectivity request handled. Success: {Success}")]
    private partial void LogConnectivityHandled(bool success);
}
