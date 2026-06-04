using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Queries.ListSesIdentities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Api.Controllers;

/// <summary>
/// Provides read-only access to AWS SES: listing the identities (email addresses and domains)
/// available on the backend.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/services/ses")]
public partial class SesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SesController"/> class.
    /// </summary>
    /// <param name="sender">The sender used to dispatch queries.</param>
    /// <param name="logger">The logger.</param>
    public SesController(ISender sender, ILogger<SesController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Lists the SES identities available on the backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the identity summaries.</returns>
    [HttpGet("identities")]
    [ProducesResponseType(typeof(SesIdentityListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListIdentities(CancellationToken cancellationToken)
    {
        LogHandlingListIdentities();
        var result = await _sender.Send(new ListSesIdentitiesQuery(), cancellationToken);
        LogListIdentitiesHandled(result.IsSuccess);
        return result.Match(
            identities => Results.Ok(new SesIdentityListResponse(
                identities.Identities
                    .Select(identity => new SesIdentitySummaryResponse(
                        identity.Identity,
                        identity.IdentityType,
                        identity.VerificationStatus))
                    .ToList())),
            error => error.AsHttpResult());
    }

    [LoggerMessage(LogLevel.Trace, "Listing SES identities.")]
    private partial void LogHandlingListIdentities();

    [LoggerMessage(LogLevel.Trace, "SES identity list handled. Success: {Success}")]
    private partial void LogListIdentitiesHandled(bool success);
}
