namespace Foundation.Domain.CertificateManager;

/// <summary>
/// The details required to request a new ACM certificate.
/// </summary>
/// <param name="DomainName">The fully qualified domain name the certificate should secure.</param>
/// <param name="ValidationMethod">The validation method, such as <c>DNS</c> or <c>EMAIL</c>.</param>
/// <param name="SubjectAlternativeNames">Additional domain names the certificate should cover, or an empty list when none are required.</param>
public sealed record CertificateRequestSpecification(
    string DomainName,
    string ValidationMethod,
    IReadOnlyList<string> SubjectAlternativeNames);
