using Foundation.Application.Cognito;
using Foundation.Domain.Search;

namespace Foundation.Infrastructure.Search;

/// <summary>
/// Contributes Amazon Cognito user pools to the global search index. Failures are swallowed and
/// reported as an empty list so a backend that is unavailable or unsupported cannot abort a full
/// index rebuild.
/// </summary>
internal sealed class CognitoResourceSource : IResourceSource
{
    private readonly ICognitoClient _client;

    public CognitoResourceSource(ICognitoClient client)
        => _client = client;

    /// <inheritdoc />
    public string ServiceKey => "cognito";

    /// <inheritdoc />
    public async Task<IReadOnlyList<SearchEntry>> ListAsync(CancellationToken cancellationToken)
    {
        var userPools = await _client.ListUserPoolsAsync(cancellationToken);
        if (!userPools.IsSuccess)
        {
            return [];
        }

        return userPools.Value
            .Select(userPool => new SearchEntry(
                ServiceKey,
                userPool.Id,
                userPool.Name,
                $"/services/cognito/{Uri.EscapeDataString(userPool.Id)}"))
            .ToList();
    }
}
