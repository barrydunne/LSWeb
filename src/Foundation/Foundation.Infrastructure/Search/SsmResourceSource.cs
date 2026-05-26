using Foundation.Application.Ssm;
using Foundation.Domain.Search;

namespace Foundation.Infrastructure.Search;

/// <summary>
/// Contributes SSM parameters to the global search index by browsing the full hierarchy from the
/// root. Failures are swallowed and reported as an empty list so a backend that is unavailable or
/// unsupported cannot abort a full index rebuild.
/// </summary>
internal sealed class SsmResourceSource : IResourceSource
{
    private readonly ISsmClient _client;

    public SsmResourceSource(ISsmClient client)
        => _client = client;

    /// <inheritdoc />
    public string ServiceKey => "ssm-parameter-store";

    /// <inheritdoc />
    public async Task<IReadOnlyList<SearchEntry>> ListAsync(CancellationToken cancellationToken)
    {
        var parameters = await _client.GetParametersByPathAsync("/", recursive: true, cancellationToken);
        if (!parameters.IsSuccess)
        {
            return [];
        }

        return parameters.Value
            .Select(parameter => new SearchEntry(
                ServiceKey,
                parameter.Name,
                parameter.Name,
                $"/services/ssm-parameter-store/{Uri.EscapeDataString(parameter.Name)}"))
            .ToList();
    }
}
