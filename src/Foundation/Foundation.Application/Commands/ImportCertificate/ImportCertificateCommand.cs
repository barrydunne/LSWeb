using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.ImportCertificate;

/// <summary>
/// Import an external certificate and its private key into ACM.
/// </summary>
/// <param name="Certificate">The PEM-encoded certificate body to import.</param>
/// <param name="PrivateKey">The PEM-encoded private key that matches the certificate.</param>
/// <param name="CertificateChain">The PEM-encoded certificate chain, or <see langword="null"/> when no chain is supplied.</param>
public record ImportCertificateCommand(
    string Certificate,
    string PrivateKey,
    string? CertificateChain) : ICommand<string>;
