using Foundation.Application.ApiGateway;
using Foundation.Domain.Search;

namespace Foundation.Infrastructure.Search;

/// <summary>
/// Contributes API Gateway REST APIs to the global search index. Failures are swallowed and
/// reported as an empty list so a backend that is unavailable or unsupported cannot abort a full
/// index rebuild.
/// </summary>
internal sealed class ApiGatewayResourceSource : IResourceSource
{
    private readonly IApiGatewayClient _client;

    public ApiGatewayResourceSource(IApiGatewayClient client)
        => _client = client;

    /// <inheritdoc />
    public string ServiceKey => "apigateway";

    /// <inheritdoc />
    public async Task<IReadOnlyList<SearchEntry>> ListAsync(CancellationToken cancellationToken)
    {
        var restApis = await _client.ListRestApisAsync(cancellationToken);
        if (!restApis.IsSuccess)
        {
            return [];
        }

        return restApis.Value
            .Select(restApi => new SearchEntry(
                ServiceKey,
                restApi.Id,
                restApi.Name,
                $"/services/apigateway/{Uri.EscapeDataString(restApi.Id)}"))
            .ToList();
    }
}
