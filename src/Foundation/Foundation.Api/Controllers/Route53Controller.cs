using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Queries.ListHostedZones;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Api.Controllers;

/// <summary>
/// Provides read-only access to AWS Route 53: listing the hosted zones available on the backend.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/services/route53")]
public partial class Route53Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Route53Controller"/> class.
    /// </summary>
    /// <param name="sender">The sender used to dispatch queries.</param>
    /// <param name="logger">The logger.</param>
    public Route53Controller(ISender sender, ILogger<Route53Controller> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Lists the Route 53 hosted zones available on the backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the hosted zone summaries.</returns>
    [HttpGet("hostedzones")]
    [ProducesResponseType(typeof(HostedZoneListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListHostedZones(CancellationToken cancellationToken)
    {
        LogHandlingListHostedZones();
        var result = await _sender.Send(new ListHostedZonesQuery(), cancellationToken);
        LogListHostedZonesHandled(result.IsSuccess);
        return result.Match(
            hostedZones => Results.Ok(new HostedZoneListResponse(
                hostedZones.HostedZones
                    .Select(hostedZone => new HostedZoneSummaryResponse(
                        hostedZone.Id,
                        hostedZone.Name,
                        hostedZone.RecordCount,
                        hostedZone.PrivateZone))
                    .ToList())),
            error => error.AsHttpResult());
    }

    [LoggerMessage(LogLevel.Trace, "Listing Route 53 hosted zones.")]
    private partial void LogHandlingListHostedZones();

    [LoggerMessage(LogLevel.Trace, "Route 53 hosted zone list handled. Success: {Success}")]
    private partial void LogListHostedZonesHandled(bool success);
}
