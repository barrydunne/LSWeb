using AspNet.KickStarter.FunctionalResult.Extensions;
using Foundation.Api.Models;
using Foundation.Application.Commands.ImportCertificate;
using Foundation.Application.Commands.RequestCertificate;
using Foundation.Application.Queries.ListCertificates;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Api.Controllers;

/// <summary>
/// Provides access to AWS Certificate Manager (ACM): listing the certificates available on the
/// backend and importing external certificate material.
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

    /// <summary>
    /// Imports an external certificate and its private key into ACM.
    /// </summary>
    /// <param name="request">The certificate material to import.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result carrying the ARN of the imported certificate.</returns>
    [HttpPost("certificates/import")]
    [ProducesResponseType(typeof(CertificateImportResponse), StatusCodes.Status201Created)]
    public async Task<IResult> ImportCertificate(
        [FromBody] CertificateImportRequest request, CancellationToken cancellationToken)
    {
        LogHandlingImportCertificate();
        var result = await _sender.Send(
            new ImportCertificateCommand(request.Certificate, request.PrivateKey, request.CertificateChain),
            cancellationToken);
        LogImportCertificateHandled(result.IsSuccess);
        return result.Match(
            arn => Results.Created(
                "/api/services/acm/certificates", new CertificateImportResponse(arn)),
            error => error.AsHttpResult());
    }

    /// <summary>
    /// Requests a new certificate from ACM for the supplied domain.
    /// </summary>
    /// <param name="request">The certificate request details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An HTTP 201 result carrying the ARN of the requested certificate.</returns>
    [HttpPost("certificates")]
    [ProducesResponseType(typeof(CertificateRequestResponse), StatusCodes.Status201Created)]
    public async Task<IResult> RequestCertificate(
        [FromBody] CertificateRequestRequest request, CancellationToken cancellationToken)
    {
        LogHandlingRequestCertificate();
        var result = await _sender.Send(
            new RequestCertificateCommand(
                request.DomainName,
                request.ValidationMethod,
                request.SubjectAlternativeNames ?? []),
            cancellationToken);
        LogRequestCertificateHandled(result.IsSuccess);
        return result.Match(
            arn => Results.Created(
                "/api/services/acm/certificates", new CertificateRequestResponse(arn)),
            error => error.AsHttpResult());
    }

    [LoggerMessage(LogLevel.Trace, "Listing ACM certificates.")]
    private partial void LogHandlingListCertificates();

    [LoggerMessage(LogLevel.Trace, "ACM certificate list handled. Success: {Success}")]
    private partial void LogListCertificatesHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Importing ACM certificate.")]
    private partial void LogHandlingImportCertificate();

    [LoggerMessage(LogLevel.Trace, "ACM certificate import handled. Success: {Success}")]
    private partial void LogImportCertificateHandled(bool success);

    [LoggerMessage(LogLevel.Trace, "Requesting ACM certificate.")]
    private partial void LogHandlingRequestCertificate();

    [LoggerMessage(LogLevel.Trace, "ACM certificate request handled. Success: {Success}")]
    private partial void LogRequestCertificateHandled(bool success);
}
