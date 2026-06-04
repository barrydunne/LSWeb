using AspNet.KickStarter.FunctionalResult;
using Foundation.Domain.CertificateManager;

namespace Foundation.Application.CertificateManager;

/// <summary>
/// Abstracts the ACM operations the application needs so the handlers stay free of any direct AWS
/// SDK dependency. The implementation flows every call through the resilient AWS gateway and
/// translates failures into a <see cref="Result"/> rather than throwing.
/// </summary>
public interface ICertificateManagerClient
{
    /// <summary>
    /// List the certificates available on the backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The certificates, or an error when the backend cannot be reached.</returns>
    Task<Result<IReadOnlyList<Certificate>>> ListCertificatesAsync(
        CancellationToken cancellationToken);
}
