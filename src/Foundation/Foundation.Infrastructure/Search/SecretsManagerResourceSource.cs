using Foundation.Application.SecretsManager;
using Foundation.Domain.Search;

namespace Foundation.Infrastructure.Search;

/// <summary>
/// Contributes Secrets Manager secrets to the global search index. Failures are swallowed and
/// reported as an empty list so a backend that is unavailable or unsupported cannot abort a full
/// index rebuild.
/// </summary>
internal sealed class SecretsManagerResourceSource : IResourceSource
{
    private readonly ISecretsManagerClient _client;

    public SecretsManagerResourceSource(ISecretsManagerClient client)
        => _client = client;

    /// <inheritdoc />
    public string ServiceKey => "secrets-manager";

    /// <inheritdoc />
    public async Task<IReadOnlyList<SearchEntry>> ListAsync(CancellationToken cancellationToken)
    {
        var secrets = await _client.ListSecretsAsync(cancellationToken);
        if (!secrets.IsSuccess)
        {
            return [];
        }

        return secrets.Value
            .Select(secret => new SearchEntry(
                ServiceKey,
                secret.Name,
                secret.Name,
                $"/services/secrets-manager/{Uri.EscapeDataString(secret.Name)}"))
            .ToList();
    }
}
