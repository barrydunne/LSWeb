using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Commands.RefreshCatalogue;
using Foundation.Application.Queries.GenerateCliSnippet;
using Foundation.Application.Queries.GetActivity;
using Foundation.Application.Queries.GetCatalogue;
using Foundation.Application.Queries.GetCircuitStatus;
using Foundation.Application.Queries.GetConnectivity;
using Foundation.Application.Queries.GetDiagnostics;
using Foundation.Application.Queries.GetHealth;
using Foundation.Application.Queries.GetLiveness;
using Foundation.Domain.Capabilities;
using Foundation.Domain.Snippets;
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
    /// Reports whether any service's AWS gateway circuit breaker is currently open.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the circuit-breaker status and any affected services.</returns>
    [HttpGet("circuit")]
    [ProducesResponseType(typeof(CircuitStatusResponse), StatusCodes.Status200OK)]
    public async Task<IResult> Circuit(CancellationToken cancellationToken)
    {
        LogHandlingCircuit();
        var result = await _sender.Send(new GetCircuitStatusQuery(), cancellationToken);
        LogCircuitHandled(result.IsSuccess);
        return result.Match(
            status => Results.Ok(new CircuitStatusResponse(status.IsOpen, status.AffectedServices)),
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
    /// Reports a diagnostic snapshot of the resolved configuration and backend connectivity.
    /// </summary>
    /// <param name="reveal">Whether to request that sensitive values be unmasked; honoured only when the host permits it.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the diagnostics snapshot with sensitive values masked by default.</returns>
    [HttpGet("diagnostics")]
    [ProducesResponseType(typeof(DiagnosticsResponse), StatusCodes.Status200OK)]
    public async Task<IResult> Diagnostics([FromQuery] bool reveal, CancellationToken cancellationToken)
    {
        LogHandlingDiagnostics(reveal);
        var result = await _sender.Send(new GetDiagnosticsQuery(reveal), cancellationToken);
        LogDiagnosticsHandled(result.IsSuccess);
        return result.Match(
            diagnostics => Results.Ok(new DiagnosticsResponse(
                diagnostics.Configuration
                    .Select(value => new DiagnosticsConfigResponse(value.Name, value.Value, value.Source, value.IsSensitive))
                    .ToList(),
                diagnostics.Endpoint,
                diagnostics.Region,
                diagnostics.ConnectivityStatus,
                diagnostics.ConnectivityError,
                diagnostics.RevealAllowed)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Generates a runnable AWS CLI snippet that reproduces an operation against the configured endpoint.
    /// </summary>
    /// <param name="request">The operation to reproduce, including any parameters.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the generated CLI command with sensitive values masked.</returns>
    [HttpPost("cli-snippet")]
    [ProducesResponseType(typeof(CliSnippetResponse), StatusCodes.Status200OK)]
    public async Task<IResult> CliSnippet([FromBody] CliSnippetRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCliSnippet(request.Service, request.Operation);
        var parameters = (request.Parameters ?? [])
            .Select(parameter => new CliParameter(parameter.Name, parameter.Value, parameter.IsSensitive))
            .ToList();
        var result = await _sender.Send(
            new GenerateCliSnippetQuery(request.Service, request.Operation, parameters),
            cancellationToken);
        LogCliSnippetHandled(result.IsSuccess);
        return result.Match(
            snippet => Results.Ok(new CliSnippetResponse(snippet.Command)),
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

    [LoggerMessage(LogLevel.Trace, "Handling circuit status request.")]
    private partial void LogHandlingCircuit();

    [LoggerMessage(LogLevel.Trace, "Circuit status request handled. Success: {Success}")]
    private partial void LogCircuitHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling connectivity request.")]
    private partial void LogHandlingConnectivity();

    [LoggerMessage(LogLevel.Trace, "Connectivity request handled. Success: {Success}")]
    private partial void LogConnectivityHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling diagnostics request. Reveal requested: {Reveal}")]
    private partial void LogHandlingDiagnostics(bool reveal);

    [LoggerMessage(LogLevel.Trace, "Diagnostics request handled. Success: {Success}")]
    private partial void LogDiagnosticsHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling CLI snippet request for {Service} {Operation}.")]
    private partial void LogHandlingCliSnippet(string service, string operation);

    [LoggerMessage(LogLevel.Trace, "CLI snippet request handled. Success: {Success}")]
    private partial void LogCliSnippetHandled(bool success);

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
