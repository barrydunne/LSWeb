using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Commands.CreateParameter;
using Foundation.Application.Commands.DeleteParameter;
using Foundation.Application.Commands.UpdateParameterValue;
using Foundation.Application.Queries.BrowseParameters;
using Foundation.Application.Queries.GetParameterHistory;
using Foundation.Application.Queries.GetParameterValue;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Api.Controllers;

/// <summary>
/// Provides access to AWS SSM Parameter Store: browsing the parameter hierarchy by path, creating a
/// new parameter, and deleting an existing one.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/services/ssm-parameter-store")]
public partial class SsmController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SsmController"/> class.
    /// </summary>
    /// <param name="sender">The sender used to dispatch queries and commands.</param>
    /// <param name="logger">The logger.</param>
    public SsmController(ISender sender, ILogger<SsmController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Browses the SSM parameters that live under a hierarchy path, optionally descending into the
    /// full tree beneath it.
    /// </summary>
    /// <param name="path">The hierarchy path to browse. Defaults to the root path.</param>
    /// <param name="recursive">Whether to include parameters in nested paths beneath the path.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the parameter summaries.</returns>
    [HttpGet("parameters")]
    [ProducesResponseType(typeof(ParameterListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> BrowseParameters(
        [FromQuery] string? path, [FromQuery] bool recursive, CancellationToken cancellationToken)
    {
        var requestedPath = string.IsNullOrWhiteSpace(path) ? "/" : path;
        LogHandlingBrowseParameters(requestedPath, recursive);
        var result = await _sender.Send(
            new BrowseParametersQuery(requestedPath, recursive), cancellationToken);
        LogBrowseParametersHandled(result.IsSuccess);
        return result.Match(
            parameters => Results.Ok(new ParameterListResponse(
                parameters.Path,
                parameters.Parameters
                    .Select(parameter => new ParameterSummaryResponse(
                        parameter.Name,
                        parameter.Type,
                        parameter.Version,
                        parameter.LastModifiedDate,
                        parameter.Arn))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Creates or overwrites an SSM parameter with the supplied name, type, value, and description.
    /// </summary>
    /// <param name="request">The parameter to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result locating the created parameter.</returns>
    [HttpPost("parameters")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IResult> CreateParameter(
        [FromBody] ParameterCreateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingCreateParameter(request.Name);
        var result = await _sender.Send(
            new CreateParameterCommand(request.Name, request.Type, request.Value, request.Description),
            cancellationToken);
        LogCreateParameterHandled(result.IsSuccess);
        return result.Match(
            () => Results.Created(
                $"/api/services/ssm-parameter-store/parameters?name={Uri.EscapeDataString(request.Name)}", null),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes an SSM parameter by name. This is a destructive action that cannot be undone.
    /// </summary>
    /// <param name="name">The fully-qualified name of the parameter to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpDelete("parameters")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteParameter(
        [FromQuery] string name, CancellationToken cancellationToken)
    {
        LogHandlingDeleteParameter(name);
        var result = await _sender.Send(new DeleteParameterCommand(name), cancellationToken);
        LogDeleteParameterHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Reads the current value of an SSM parameter. SecureString values are masked by default; the
    /// unmasked value is only returned when a reveal is both requested and permitted by the host.
    /// </summary>
    /// <param name="name">The fully-qualified name of the parameter to read.</param>
    /// <param name="reveal">Whether the caller is requesting the unmasked value.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the parameter value.</returns>
    [HttpGet("parameters/value")]
    [ProducesResponseType(typeof(ParameterValueResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetParameterValue(
        [FromQuery] string name, [FromQuery] bool reveal, CancellationToken cancellationToken)
    {
        LogHandlingGetParameterValue(name, reveal);
        var result = await _sender.Send(new GetParameterValueQuery(name, reveal), cancellationToken);
        LogGetParameterValueHandled(result.IsSuccess);
        return result.Match(
            value => Results.Ok(new ParameterValueResponse(
                value.Name,
                value.Type,
                value.Version,
                value.Value,
                value.IsSensitive,
                value.RevealAllowed)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Stores a new value against an existing SSM parameter, creating a new version while preserving
    /// the parameter's existing type.
    /// </summary>
    /// <param name="name">The fully-qualified name of the parameter to update.</param>
    /// <param name="request">The new value to store.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result on success.</returns>
    [HttpPut("parameters/value")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> UpdateParameterValue(
        [FromQuery] string name, [FromBody] ParameterValueUpdateRequest request, CancellationToken cancellationToken)
    {
        LogHandlingUpdateParameterValue(name);
        var result = await _sender.Send(
            new UpdateParameterValueCommand(name, request.Value), cancellationToken);
        LogUpdateParameterValueHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Reads the change history of an SSM parameter. SecureString values are masked by default; the
    /// unmasked values are only returned when a reveal is both requested and permitted by the host.
    /// </summary>
    /// <param name="name">The fully-qualified name of the parameter to read.</param>
    /// <param name="reveal">Whether the caller is requesting the unmasked values.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the parameter history.</returns>
    [HttpGet("parameters/history")]
    [ProducesResponseType(typeof(ParameterHistoryResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetParameterHistory(
        [FromQuery] string name, [FromQuery] bool reveal, CancellationToken cancellationToken)
    {
        LogHandlingGetParameterHistory(name, reveal);
        var result = await _sender.Send(new GetParameterHistoryQuery(name, reveal), cancellationToken);
        LogGetParameterHistoryHandled(result.IsSuccess);
        return result.Match(
            history => Results.Ok(new ParameterHistoryResponse(
                history.Name,
                history.RevealAllowed,
                history.Entries
                    .Select(entry => new ParameterHistoryEntryResponse(
                        entry.Type,
                        entry.Version,
                        entry.Value,
                        entry.LastModifiedDate,
                        entry.LastModifiedUser,
                        entry.IsSensitive))
                    .ToList())),
            error => error.AsHttpResult());
    }

    [LoggerMessage(LogLevel.Trace, "Handling SSM parameter browse request for {Path}. Recursive: {Recursive}")]
    private partial void LogHandlingBrowseParameters(string path, bool recursive);

    [LoggerMessage(LogLevel.Trace, "SSM parameter browse request handled. Success: {Success}")]
    private partial void LogBrowseParametersHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling SSM parameter create request for {Name}.")]
    private partial void LogHandlingCreateParameter(string name);

    [LoggerMessage(LogLevel.Trace, "SSM parameter create request handled. Success: {Success}")]
    private partial void LogCreateParameterHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling SSM parameter delete request for {Name}.")]
    private partial void LogHandlingDeleteParameter(string name);

    [LoggerMessage(LogLevel.Trace, "SSM parameter delete request handled. Success: {Success}")]
    private partial void LogDeleteParameterHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling SSM parameter value request for {Name}. Reveal: {Reveal}")]
    private partial void LogHandlingGetParameterValue(string name, bool reveal);

    [LoggerMessage(LogLevel.Trace, "SSM parameter value request handled. Success: {Success}")]
    private partial void LogGetParameterValueHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling SSM parameter value update request for {Name}.")]
    private partial void LogHandlingUpdateParameterValue(string name);

    [LoggerMessage(LogLevel.Trace, "SSM parameter value update request handled. Success: {Success}")]
    private partial void LogUpdateParameterValueHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling SSM parameter history request for {Name}. Reveal: {Reveal}")]
    private partial void LogHandlingGetParameterHistory(string name, bool reveal);

    [LoggerMessage(LogLevel.Trace, "SSM parameter history request handled. Success: {Success}")]
    private partial void LogGetParameterHistoryHandled(bool success);
}
