using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Queries.ListCertificates;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Api.Controllers;

/// <summary>
/// Provides read-only access to AWS Certificate Manager (ACM): listing the certificates available
/// on the backend.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/services/acm")]
public partial class CertificateManagerController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CertificateManagerController"/> class.
    /// </summary>
    /// <param name="sender">The sender used to dispatch queries.</param>
    /// <param name="logger">The logger.</param>
    public CertificateManagerController(ISender sender, ILogger<CertificateManagerController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Lists the ACM certificates available on the backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 200 result carrying the certificate summaries.</returns>
    [HttpGet("certificates")]
    [ProducesResponseType(typeof(CertificateListResponse), StatusCodes.Status200OK)]
    public async Task<IResult> ListCertificates(CancellationToken cancellationToken)
    {
        LogHandlingListCertificates();
        var result = await _sender.Send(new ListCertificatesQuery(), cancellationToken);
        LogListCertificatesHandled(result.IsSuccess);
        return result.Match(
            certificates => Results.Ok(new CertificateListResponse(
                certificates.Certificates
                    .Select(certificate => new CertificateSummaryResponse(
                        certificate.Arn,
                        certificate.DomainName,
                        certificate.Status,
                        certificate.Type))
                    .ToList())),
            error => error.AsHttpResult());
    }

    [LoggerMessage(LogLevel.Trace, "Listing ACM certificates.")]
    private partial void LogHandlingListCertificates();

    [LoggerMessage(LogLevel.Trace, "ACM certificate list handled. Success: {Success}")]
    private partial void LogListCertificatesHandled(bool success);
}
