using Foundation.Application.DynamoDb;
using Foundation.Domain.Search;

namespace Foundation.Infrastructure.Search;

/// <summary>
/// Contributes DynamoDB tables to the global search index. Failures are swallowed and reported as an
/// empty list so a backend that is unavailable or unsupported cannot abort a full index rebuild.
/// </summary>
internal sealed class DynamoDbResourceSource : IResourceSource
{
    private readonly IDynamoDbClient _client;

    public DynamoDbResourceSource(IDynamoDbClient client)
        => _client = client;

    /// <inheritdoc />
    public string ServiceKey => "dynamodb";

    /// <inheritdoc />
    public async Task<IReadOnlyList<SearchEntry>> ListAsync(CancellationToken cancellationToken)
    {
        var tables = await _client.ListTablesAsync(cancellationToken);
        if (!tables.IsSuccess)
        {
            return [];
        }

        return tables.Value
            .Select(table => new SearchEntry(
                ServiceKey,
                table.Name,
                table.Name,
                $"/services/dynamodb/{Uri.EscapeDataString(table.Name)}"))
            .ToList();
    }
}
