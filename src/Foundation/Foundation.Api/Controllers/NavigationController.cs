using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Queries.ResolveReference;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Api.Controllers;

/// <summary>
/// Provides cross-resource navigation endpoints such as reference resolution.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/navigation")]
public partial class NavigationController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationController"/> class.
    /// </summary>
    /// <param name="sender">The sender used to dispatch queries.</param>
    /// <param name="logger">The logger.</param>
    public NavigationController(ISender sender, ILogger<NavigationController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Resolves an ARN or resource identifier to the console route for the target resource.
    /// </summary>
    /// <param name="reference">The ARN or resource identifier to resolve, supplied as the <c>ref</c> query parameter.</param>
    /// <param name="service">An optional service hint used when the reference is a bare identifier rather than an ARN.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the resolved route, or an error status when the reference cannot be resolved.</returns>
    [HttpGet("resolve")]
    [ProducesResponseType(typeof(ResolveReferenceResponse), StatusCodes.Status200OK)]
    public async Task<IResult> Resolve(
        [FromQuery(Name = "ref")] string reference,
        [FromQuery(Name = "service")] string? service,
        CancellationToken cancellationToken)
    {
        LogHandlingResolve(reference);
        var result = await _sender.Send(new ResolveReferenceQuery(reference, service), cancellationToken);
        LogResolveHandled(result.IsSuccess);
        return result.Match(
            resolved => Results.Ok(new ResolveReferenceResponse(
                resolved.ServiceKey,
                resolved.ResourceId,
                resolved.Route)),
            error => error.AsHttpResult());
    }

    [LoggerMessage(LogLevel.Trace, "Handling reference resolution for {Reference}.")]
    private partial void LogHandlingResolve(string reference);

    [LoggerMessage(LogLevel.Trace, "Reference resolution handled. Success: {Success}")]
    private partial void LogResolveHandled(bool success);
}
