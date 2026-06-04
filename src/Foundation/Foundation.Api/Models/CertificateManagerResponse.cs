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
