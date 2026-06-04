using Foundation.Application.EventBridge;
using Foundation.Domain.Search;

namespace Foundation.Infrastructure.Search;

/// <summary>
/// Contributes EventBridge rules to the global search index. Failures are swallowed and reported as
/// an empty list so a backend that is unavailable or unsupported cannot abort a full index rebuild.
/// </summary>
internal sealed class EventBridgeResourceSource : IResourceSource
{
    private readonly IEventBridgeClient _client;

    public EventBridgeResourceSource(IEventBridgeClient client)
        => _client = client;

    /// <inheritdoc />
    public string ServiceKey => "eventbridge";

    /// <inheritdoc />
    public async Task<IReadOnlyList<SearchEntry>> ListAsync(CancellationToken cancellationToken)
    {
        var rules = await _client.ListRulesAsync(cancellationToken);
        if (!rules.IsSuccess)
        {
            return [];
        }

        return rules.Value
            .Select(rule => new SearchEntry(
                ServiceKey,
                rule.Name,
                rule.Name,
                $"/services/eventbridge/{Uri.EscapeDataString(rule.Name)}"))
            .ToList();
    }
}
