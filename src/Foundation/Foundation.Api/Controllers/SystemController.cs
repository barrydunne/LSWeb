using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Commands.RefreshCatalogue;
using Foundation.Application.Queries.GetActivity;
using Foundation.Application.Queries.GetCatalogue;
using Foundation.Application.Queries.GetConnectivity;
using Foundation.Application.Queries.GetHealth;
using Foundation.Application.Queries.GetLiveness;
using Foundation.Domain.Capabilities;
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
    [HttpGet("liveness")]
    [ProducesResponseType(typeof(GetLivenessQueryResult), StatusCodes.Status200OK)]
    public async Task<IResult> Liveness(CancellationToken cancellationToken)
    {
        LogHandlingLiveness();
        var result = await _sender.Send(new GetLivenessQuery(), cancellationToken);
        LogLivenessHandled(result.IsSuccess);
        return result.Match(
            status => Results.Ok(status),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Reports the availability of each managed AWS service from the latest health snapshot.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the per-service availability snapshot.</returns>
    [HttpGet("health")]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public async Task<IResult> Health(CancellationToken cancellationToken)
    {
        LogHandlingHealth();
        var result = await _sender.Send(new GetHealthQuery(), cancellationToken);
        LogHealthHandled(result.IsSuccess);
        return result.Match(
            health => Results.Ok(new HealthResponse(
                health.Services
                    .Select(service => new ServiceHealthResponse(service.Key, service.Availability.ToString()))
                    .ToList())),
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

    /// <summary>
    /// Lists the managed AWS services available in the console.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the catalogue of managed services.</returns>
    [HttpGet("catalogue")]
    [ProducesResponseType(typeof(CatalogueResponse), StatusCodes.Status200OK)]
    public async Task<IResult> Catalogue(CancellationToken cancellationToken)
    {
        LogHandlingCatalogue();
        var result = await _sender.Send(new GetCatalogueQuery(), cancellationToken);
        LogCatalogueHandled(result.IsSuccess);
        return result.Match(
            catalogue => Results.Ok(new CatalogueResponse(
                catalogue.Services
                    .Select(service =>
                    {
                        var entry = catalogue.Capabilities.Find(service.Key);
                        var supported = entry is null || entry.Status != CapabilityStatus.Unsupported;
                        return new CatalogueServiceResponse(
                            service.Key,
                            service.DisplayName,
                            service.Category.ToString(),
                            service.IconHint,
                            service.Route,
                            supported,
                            supported ? null : entry!.Detail);
                    })
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Refreshes the managed service catalogue, broadcasting progress to connected clients.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 202 result acknowledging that the refresh has been accepted.</returns>
    [HttpPost("catalogue/refresh")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IResult> RefreshCatalogue(CancellationToken cancellationToken)
    {
        LogHandlingCatalogueRefresh();
        var result = await _sender.Send(new RefreshCatalogueCommand(), cancellationToken);
        LogCatalogueRefreshHandled(result.IsSuccess);
        return result.Match(
            () => Results.Accepted(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Lists the in-session activity log of completed backend operations.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the recorded activity entries, most recent first.</returns>
    [HttpGet("activity")]
    [ProducesResponseType(typeof(ActivityResponse), StatusCodes.Status200OK)]
    public async Task<IResult> Activity(CancellationToken cancellationToken)
    {
        LogHandlingActivity();
        var result = await _sender.Send(new GetActivityQuery(), cancellationToken);
        LogActivityHandled(result.IsSuccess);
        return result.Match(
            activity => Results.Ok(new ActivityResponse(
                activity.Entries
                    .Select(entry => new ActivityEntryResponse(
                        entry.OperationId,
                        entry.Operation,
                        entry.State.ToString(),
                        entry.Message,
                        entry.OccurredAt))
                    .ToList())),
            error => error.AsHttpResult());
    }

    [LoggerMessage(LogLevel.Trace, "Handling liveness request.")]
    private partial void LogHandlingLiveness();

    [LoggerMessage(LogLevel.Trace, "Liveness request handled. Success: {Success}")]
    private partial void LogLivenessHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling health request.")]
    private partial void LogHandlingHealth();

    [LoggerMessage(LogLevel.Trace, "Health request handled. Success: {Success}")]
    private partial void LogHealthHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling connectivity request.")]
    private partial void LogHandlingConnectivity();

    [LoggerMessage(LogLevel.Trace, "Connectivity request handled. Success: {Success}")]
    private partial void LogConnectivityHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling catalogue request.")]
    private partial void LogHandlingCatalogue();

    [LoggerMessage(LogLevel.Trace, "Catalogue request handled. Success: {Success}")]
    private partial void LogCatalogueHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling catalogue refresh request.")]
    private partial void LogHandlingCatalogueRefresh();

    [LoggerMessage(LogLevel.Trace, "Catalogue refresh request handled. Success: {Success}")]
    private partial void LogCatalogueRefreshHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling activity request.")]
    private partial void LogHandlingActivity();

    [LoggerMessage(LogLevel.Trace, "Activity request handled. Success: {Success}")]
    private partial void LogActivityHandled(bool success);
}
