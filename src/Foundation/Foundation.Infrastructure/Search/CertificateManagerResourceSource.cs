using Foundation.Application.CertificateManager;
using Foundation.Domain.Search;

namespace Foundation.Infrastructure.Search;

/// <summary>
/// Contributes ACM certificates to the global search index. Failures are swallowed and reported as
/// an empty list so a backend that is unavailable or unsupported cannot abort a full index rebuild.
/// </summary>
internal sealed class CertificateManagerResourceSource : IResourceSource
{
    private readonly ICertificateManagerClient _client;

    public CertificateManagerResourceSource(ICertificateManagerClient client)
        => _client = client;

    /// <inheritdoc />
    public string ServiceKey => "acm";

    /// <inheritdoc />
    public async Task<IReadOnlyList<SearchEntry>> ListAsync(CancellationToken cancellationToken)
    {
        var certificates = await _client.ListCertificatesAsync(cancellationToken);
        if (!certificates.IsSuccess)
        {
            return [];
        }

        return certificates.Value
            .Select(certificate => new SearchEntry(
                ServiceKey,
                certificate.Arn,
                certificate.DomainName,
                $"/services/acm/{Uri.EscapeDataString(certificate.Arn)}"))
            .ToList();
    }
}
