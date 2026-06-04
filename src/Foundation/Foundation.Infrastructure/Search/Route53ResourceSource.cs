using Foundation.Application.Route53;
using Foundation.Domain.Search;

namespace Foundation.Infrastructure.Search;

/// <summary>
/// Contributes Route 53 hosted zones to the global search index. Failures are swallowed and
/// reported as an empty list so a backend that is unavailable or unsupported cannot abort a full
/// index rebuild.
/// </summary>
internal sealed class Route53ResourceSource : IResourceSource
{
    private readonly IRoute53Client _client;

    public Route53ResourceSource(IRoute53Client client)
        => _client = client;

    /// <inheritdoc />
    public string ServiceKey => "route53";

    /// <inheritdoc />
    public async Task<IReadOnlyList<SearchEntry>> ListAsync(CancellationToken cancellationToken)
    {
        var hostedZones = await _client.ListHostedZonesAsync(cancellationToken);
        if (!hostedZones.IsSuccess)
        {
            return [];
        }

        return hostedZones.Value
            .Select(hostedZone => new SearchEntry(
                ServiceKey,
                hostedZone.Id,
                hostedZone.Name,
                $"/services/route53/{Uri.EscapeDataString(hostedZone.Id)}"))
            .ToList();
    }
}
