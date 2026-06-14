using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Commands.DeleteSesIdentity;
using Foundation.Application.Commands.EnableDomainDkim;
using Foundation.Application.Commands.VerifyDomainIdentity;
using Foundation.Application.Commands.VerifyEmailIdentity;
using Foundation.Application.Queries.GetSesDomainSetup;
using Foundation.Application.Queries.GetSesIdentity;
using Foundation.Application.Queries.ListSesIdentities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Api.Controllers;

/// <summary>
/// Provides access to AWS SES: listing the identities (email addresses and domains) available on
/// the backend, inspecting a single identity, and managing the email verification lifecycle.
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

    /// <summary>
    /// Gets the verification detail for a single SES identity.
    /// </summary>
    /// <param name="identity">The email address or domain name to look up.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the identity detail.</returns>
    [HttpGet("identities/{identity}")]
    [ProducesResponseType(typeof(SesIdentityDetailResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetIdentity(string identity, CancellationToken cancellationToken)
    {
        LogHandlingGetIdentity(identity);
        var result = await _sender.Send(new GetSesIdentityQuery(identity), cancellationToken);
        LogGetIdentityHandled(result.IsSuccess);
        return result.Match(
            value => Results.Ok(new SesIdentityDetailResponse(
                value.Identity.Identity,
                value.Identity.IdentityType,
                value.Identity.VerificationStatus)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Starts the verification of an SES email address identity.
    /// </summary>
    /// <param name="request">The email address to verify.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result when the verification request was accepted.</returns>
    [HttpPost("identities")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IResult> VerifyEmailIdentity(
        [FromBody] SesVerifyEmailRequest request, CancellationToken cancellationToken)
    {
        LogHandlingVerifyEmail(request.EmailAddress);
        var result = await _sender.Send(
            new VerifyEmailIdentityCommand(request.EmailAddress), cancellationToken);
        LogVerifyEmailHandled(result.IsSuccess);
        return result.Match(
            () => Results.Created(
                $"/api/services/ses/identities/{Uri.EscapeDataString(request.EmailAddress)}", null),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Deletes an SES identity (an email address or domain) from the backend.
    /// </summary>
    /// <param name="identity">The email address or domain name to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result when the identity was deleted.</returns>
    [HttpDelete("identities/{identity}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteIdentity(string identity, CancellationToken cancellationToken)
    {
        LogHandlingDeleteIdentity(identity);
        var result = await _sender.Send(new DeleteSesIdentityCommand(identity), cancellationToken);
        LogDeleteIdentityHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    [LoggerMessage(LogLevel.Trace, "Getting SES identity {Identity}.")]
    private partial void LogHandlingGetIdentity(string identity);

    [LoggerMessage(LogLevel.Trace, "SES identity get handled. Success: {Success}")]
    private partial void LogGetIdentityHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Requesting verification of SES email identity {EmailAddress}.")]
    private partial void LogHandlingVerifyEmail(string emailAddress);

    [LoggerMessage(LogLevel.Trace, "SES email identity verification handled. Success: {Success}")]
    private partial void LogVerifyEmailHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Deleting SES identity {Identity}.")]
    private partial void LogHandlingDeleteIdentity(string identity);

    [LoggerMessage(LogLevel.Trace, "SES identity delete handled. Success: {Success}")]
    private partial void LogDeleteIdentityHandled(bool success);

    /// <summary>
    /// Gets the domain verification and DKIM setup state for an SES domain identity.
    /// </summary>
    /// <param name="identity">The domain name to look up.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the domain setup state.</returns>
    [HttpGet("identities/{identity}/domain-setup")]
    [ProducesResponseType(typeof(SesDomainSetupResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetDomainSetup(string identity, CancellationToken cancellationToken)
    {
        LogHandlingGetDomainSetup(identity);
        var result = await _sender.Send(new GetSesDomainSetupQuery(identity), cancellationToken);
        LogGetDomainSetupHandled(result.IsSuccess);
        return result.Match(
            value => Results.Ok(new SesDomainSetupResponse(
                value.Setup.Domain,
                value.Setup.VerificationStatus,
                value.Setup.VerificationToken,
                value.Setup.DkimVerificationStatus,
                value.Setup.DkimTokens)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Initiates the verification of an SES domain identity.
    /// </summary>
    /// <param name="request">The domain name to verify.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result when the verification request was accepted.</returns>
    [HttpPost("domains")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IResult> VerifyDomainIdentity(
        [FromBody] SesVerifyDomainRequest request, CancellationToken cancellationToken)
    {
        LogHandlingVerifyDomain(request.Domain);
        var result = await _sender.Send(
            new VerifyDomainIdentityCommand(request.Domain), cancellationToken);
        LogVerifyDomainHandled(result.IsSuccess);
        return result.Match(
            () => Results.Created(
                $"/api/services/ses/identities/{Uri.EscapeDataString(request.Domain)}/domain-setup", null),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Enables DKIM signing for an SES domain identity.
    /// </summary>
    /// <param name="identity">The domain name to enable DKIM for.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 204 result when DKIM was enabled.</returns>
    [HttpPost("identities/{identity}/dkim")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> EnableDomainDkim(string identity, CancellationToken cancellationToken)
    {
        LogHandlingEnableDkim(identity);
        var result = await _sender.Send(new EnableDomainDkimCommand(identity), cancellationToken);
        LogEnableDkimHandled(result.IsSuccess);
        return result.Match(
            () => Results.NoContent(),
            error => error.AsHttpResult());
    }

    [LoggerMessage(LogLevel.Trace, "Getting SES domain setup for {Identity}.")]
    private partial void LogHandlingGetDomainSetup(string identity);

    [LoggerMessage(LogLevel.Trace, "SES domain setup get handled. Success: {Success}")]
    private partial void LogGetDomainSetupHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Initiating verification of SES domain identity {Domain}.")]
    private partial void LogHandlingVerifyDomain(string domain);

    [LoggerMessage(LogLevel.Trace, "SES domain identity verification handled. Success: {Success}")]
    private partial void LogVerifyDomainHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Enabling DKIM for SES domain {Identity}.")]
    private partial void LogHandlingEnableDkim(string identity);

    [LoggerMessage(LogLevel.Trace, "SES domain DKIM enable handled. Success: {Success}")]
    private partial void LogEnableDkimHandled(bool success);
}
