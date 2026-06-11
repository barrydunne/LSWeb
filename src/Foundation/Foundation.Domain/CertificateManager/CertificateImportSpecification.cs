namespace Foundation.Domain.CertificateManager;

/// <summary>
/// The certificate material to import into ACM.
/// </summary>
/// <param name="Certificate">The PEM-encoded certificate body to import.</param>
/// <param name="PrivateKey">The PEM-encoded private key that matches the certificate.</param>
/// <param name="CertificateChain">The PEM-encoded certificate chain, or <c>null</c> when no chain is supplied.</param>
public sealed record CertificateImportSpecification(
    string Certificate,
    string PrivateKey,
    string? CertificateChain);
