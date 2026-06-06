using Foundation.Application.ApiGatewayV2;
using Foundation.Domain.Search;

namespace Foundation.Infrastructure.Search;

/// <summary>
/// Contributes Amazon API Gateway v2 APIs to the global search index. Failures are swallowed and
/// reported as an empty list so a backend that is unavailable or unsupported cannot abort a full
/// index rebuild.
/// </summary>
internal sealed class ApiGatewayV2ResourceSource : IResourceSource
{
    private readonly IApiGatewayV2Client _client;

    public ApiGatewayV2ResourceSource(IApiGatewayV2Client client)
        => _client = client;

    /// <inheritdoc />
    public string ServiceKey => "apigatewayv2";

    /// <inheritdoc />
    public async Task<IReadOnlyList<SearchEntry>> ListAsync(CancellationToken cancellationToken)
    {
        var apis = await _client.ListApisAsync(cancellationToken);
        if (!apis.IsSuccess)
        {
            return [];
        }

        return apis.Value
            .Select(api => new SearchEntry(
                ServiceKey,
                api.ApiId,
                api.Name,
                $"/services/apigatewayv2/{Uri.EscapeDataString(api.ApiId)}"))
            .ToList();
    }
}
