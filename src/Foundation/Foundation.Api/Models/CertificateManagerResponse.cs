namespace Foundation.Api.Models;

/// <summary>
/// The ACM certificates available on the backend.
/// </summary>
/// <param name="Certificates">The certificate summaries, ordered as returned by the backend.</param>
public sealed record CertificateListResponse(
    IReadOnlyList<CertificateSummaryResponse> Certificates);

/// <summary>
/// A concise view of an ACM certificate as it appears in a list.
/// </summary>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the certificate.</param>
/// <param name="DomainName">The fully qualified domain name the certificate secures.</param>
/// <param name="Status">The certificate status, such as <c>ISSUED</c> or <c>PENDING_VALIDATION</c>.</param>
/// <param name="Type">The certificate type, such as <c>AMAZON_ISSUED</c> or <c>IMPORTED</c>, or <c>null</c> when not reported.</param>
public sealed record CertificateSummaryResponse(
    string Arn,
    string DomainName,
    string Status,
    string? Type);

/// <summary>
/// A request to import an external certificate and its private key into ACM.
/// </summary>
/// <param name="Certificate">The PEM-encoded certificate body to import.</param>
/// <param name="PrivateKey">The PEM-encoded private key that matches the certificate.</param>
/// <param name="CertificateChain">The PEM-encoded certificate chain, or <see langword="null"/> when no chain is supplied.</param>
public sealed record CertificateImportRequest(
    string Certificate,
    string PrivateKey,
    string? CertificateChain);

/// <summary>
/// The result of importing a certificate into ACM.
/// </summary>
/// <param name="Arn">The Amazon Resource Name of the imported certificate.</param>
public sealed record CertificateImportResponse(string Arn);

/// <summary>
/// A request to create a new ACM certificate for a domain.
/// </summary>
/// <param name="DomainName">The fully qualified domain name the certificate should secure.</param>
/// <param name="ValidationMethod">The validation method, such as <c>DNS</c> or <c>EMAIL</c>.</param>
/// <param name="SubjectAlternativeNames">Additional domain names the certificate should cover, or <see langword="null"/> when none are required.</param>
public sealed record CertificateRequestRequest(
    string DomainName,
    string ValidationMethod,
    IReadOnlyList<string>? SubjectAlternativeNames);

/// <summary>
/// The result of requesting a certificate from ACM.
/// </summary>
/// <param name="Arn">The Amazon Resource Name of the requested certificate.</param>
public sealed record CertificateRequestResponse(string Arn);
