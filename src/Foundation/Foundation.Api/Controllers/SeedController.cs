using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Commands.ApplySeedTemplate;
using Foundation.Application.Queries.GetSeedTemplates;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Api.Controllers;

/// <summary>
/// Provides endpoints for listing and applying seed templates that provision sample resources with a
/// single click.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/seed")]
public partial class SeedController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeedController"/> class.
    /// </summary>
    /// <param name="sender">The sender used to dispatch queries and commands.</param>
    /// <param name="logger">The logger.</param>
    public SeedController(ISender sender, ILogger<SeedController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Lists the available seed templates.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the catalogue of seed templates.</returns>
    [HttpGet("templates")]
    [ProducesResponseType(typeof(SeedTemplatesResponse), StatusCodes.Status200OK)]
    public async Task<IResult> Templates(CancellationToken cancellationToken)
    {
        LogHandlingTemplates();
        var result = await _sender.Send(new GetSeedTemplatesQuery(), cancellationToken);
        LogTemplatesHandled(result.IsSuccess);
        return result.Match(
            templates => Results.Ok(new SeedTemplatesResponse(
                templates.Templates
                    .Select(template => new SeedTemplateResponse(
                        template.Id,
                        template.Name,
                        template.Description,
                        template.Resources
                            .Select(resource => new SeedResourceResponse(
                                resource.ServiceKey,
                                resource.ResourceType,
                                resource.Name))
                            .ToList()))
                    .ToList())),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Applies the specified seed template, provisioning each of its resources and returning a
    /// per-resource result so that partial success is reported.
    /// </summary>
    /// <param name="templateId">The identifier of the template to apply, taken from the route.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the aggregate outcome with per-resource results.</returns>
    [HttpPost("templates/{templateId}/apply")]
    [ProducesResponseType(typeof(SeedOutcomeResponse), StatusCodes.Status200OK)]
    public async Task<IResult> Apply(string templateId, CancellationToken cancellationToken)
    {
        LogHandlingApply(templateId);
        var result = await _sender.Send(new ApplySeedTemplateCommand(templateId), cancellationToken);
        LogApplyHandled(result.IsSuccess);
        return result.Match(
            outcome => Results.Ok(new SeedOutcomeResponse(
                outcome.OperationId,
                outcome.TemplateId,
                outcome.TotalCount,
                outcome.SucceededCount,
                outcome.FailedCount,
                outcome.OverallState.ToString(),
                outcome.Items
                    .Select(item => new SeedResourceResultResponse(
                        item.ServiceKey,
                        item.ResourceType,
                        item.Name,
                        item.Succeeded,
                        item.Error))
                    .ToList())),
            error => error.AsHttpResult());
    }

    [LoggerMessage(LogLevel.Trace, "Handling seed templates list request.")]
    private partial void LogHandlingTemplates();

    [LoggerMessage(LogLevel.Trace, "Seed templates list request handled. Success: {Success}")]
    private partial void LogTemplatesHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Handling seed template apply request for {TemplateId}.")]
    private partial void LogHandlingApply(string templateId);

    [LoggerMessage(LogLevel.Trace, "Seed template apply request handled. Success: {Success}")]
    private partial void LogApplyHandled(bool success);
}
