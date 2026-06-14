using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateRoute53HostedZone;
using Foundation.Application.Commands.DeleteRoute53Record;
using Foundation.Application.Commands.UpsertRoute53Record;
using Foundation.Application.Queries.ListHostedZones;
using Foundation.Application.Queries.ListRoute53Records;
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

    /// <summary>
    /// Creates a Route 53 public hosted zone.
    /// </summary>
    /// <param name="request">The hosted zone to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result locating the hosted zone list.</returns>
    [HttpPost("hostedzones")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IResult> CreateHostedZone(
        [FromBody] HostedZoneCreateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreateHostedZone(request.Name);
        var result = await _sender.Send(
            new CreateRoute53HostedZoneCommand(request.Name, request.Comment), cancellationToken);
        LogCreateHostedZoneHandled(result.IsSuccess);
        return result.Match(
            () => Results.Created("/api/services/route53/hostedzones", null),
            error => error.AsHttpResult());
    }

    [LoggerMessage(LogLevel.Trace, "Listing Route 53 hosted zones.")]
    private partial void LogHandlingListHostedZones();

    [LoggerMessage(LogLevel.Trace, "Route 53 hosted zone list handled. Success: {Success}")]
    private partial void LogListHostedZonesHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Route 53 hosted zone create request for '{Name}'.")]
    private partial void LogHandlingCreateHostedZone(string name);

    [LoggerMessage(LogLevel.Trace, "Route 53 hosted zone create request handled. Success: {Success}")]
    private partial void LogCreateHostedZoneHandled(bool success);

    /// <summary>
    /// Lists the DNS resource record sets in a hosted zone.
    /// </summary>
    /// <param name="zoneId">The identifier of the hosted zone.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the resource record sets.</returns>
    [HttpGet("records")]
    [ProducesResponseType(typeof(Route53RecordListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListRecords([FromQuery] string zoneId, CancellationToken cancellationToken)
    {
        LogHandlingListRecords(zoneId);
        var result = await _sender.Send(new ListRoute53RecordsQuery(zoneId), cancellationToken);
        LogListRecordsHandled(result.IsSuccess);
        return result.Match(
            records => Results.Ok(new Route53RecordListResponse(
                records.Records
                    .Select(record => new Route53RecordResponse(
                        record.Name,
                        record.Type,
                        record.Ttl,
                        record.Values))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates or replaces a DNS resource record set in a hosted zone.
    /// </summary>
    /// <param name="zoneId">The identifier of the hosted zone.</param>
    /// <param name="request">The record set to create or replace.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("records")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> UpsertRecord(
        [FromQuery] string zoneId, [FromBody] Route53RecordRequest request, CancellationToken cancellationToken)
    {
        LogHandlingUpsertRecord(zoneId, request.Name);
        var result = await _sender.Send(
            new UpsertRoute53RecordCommand(zoneId, request.Name, request.Type, request.Ttl, request.Values ?? []),
            cancellationToken);
        LogUpsertRecordHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes a DNS resource record set from a hosted zone.
    /// </summary>
    /// <param name="zoneId">The identifier of the hosted zone.</param>
    /// <param name="request">The record set to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("records")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteRecord(
        [FromQuery] string zoneId, [FromBody] Route53RecordRequest request, CancellationToken cancellationToken)
    {
        LogHandlingDeleteRecord(zoneId, request.Name);
        var result = await _sender.Send(
            new DeleteRoute53RecordCommand(zoneId, request.Name, request.Type, request.Ttl, request.Values ?? []),
            cancellationToken);
        LogDeleteRecordHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    [LoggerMessage(LogLevel.Trace, "Listing Route 53 records for zone '{ZoneId}'.")]
    private partial void LogHandlingListRecords(string zoneId);

    [LoggerMessage(LogLevel.Trace, "Route 53 record list handled. Success: {Success}")]
    private partial void LogListRecordsHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Route 53 record upsert for zone '{ZoneId}' record '{Name}'.")]
    private partial void LogHandlingUpsertRecord(string zoneId, string name);

    [LoggerMessage(LogLevel.Trace, "Route 53 record upsert handled. Success: {Success}")]
    private partial void LogUpsertRecordHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling Route 53 record delete for zone '{ZoneId}' record '{Name}'.")]
    private partial void LogHandlingDeleteRecord(string zoneId, string name);

    [LoggerMessage(LogLevel.Trace, "Route 53 record delete handled. Success: {Success}")]
    private partial void LogDeleteRecordHandled(bool success);
}
