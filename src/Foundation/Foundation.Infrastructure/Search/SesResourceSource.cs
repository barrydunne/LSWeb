using Foundation.Application.Ses;
using Foundation.Domain.Search;

namespace Foundation.Infrastructure.Search;

/// <summary>
/// Contributes SES identities to the global search index. Failures are swallowed and reported as an
/// empty list so a backend that is unavailable or unsupported cannot abort a full index rebuild.
/// </summary>
internal sealed class SesResourceSource : IResourceSource
{
    private readonly ISesClient _client;

    public SesResourceSource(ISesClient client)
        => _client = client;

    /// <inheritdoc />
    public string ServiceKey => "ses";

    /// <inheritdoc />
    public async Task<IReadOnlyList<SearchEntry>> ListAsync(CancellationToken cancellationToken)
    {
        var identities = await _client.ListIdentitiesAsync(cancellationToken);
        if (!identities.IsSuccess)
        {
            return [];
        }

        return identities.Value
            .Select(identity => new SearchEntry(
                ServiceKey,
                identity.Identity,
                identity.Identity,
                $"/services/ses/{Uri.EscapeDataString(identity.Identity)}"))
            .ToList();
    }
}
