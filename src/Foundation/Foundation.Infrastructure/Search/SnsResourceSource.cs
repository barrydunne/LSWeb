using Foundation.Application.Sns;
using Foundation.Domain.Search;

namespace Foundation.Infrastructure.Search;

/// <summary>
/// Contributes SNS topics to the global search index. Failures are swallowed and reported as an
/// empty list so a backend that is unavailable or unsupported cannot abort a full index rebuild.
/// </summary>
internal sealed class SnsResourceSource : IResourceSource
{
    private readonly ISnsClient _client;

    public SnsResourceSource(ISnsClient client)
        => _client = client;

    /// <inheritdoc />
    public string ServiceKey => "sns";

    /// <inheritdoc />
    public async Task<IReadOnlyList<SearchEntry>> ListAsync(CancellationToken cancellationToken)
    {
        var topics = await _client.ListTopicsAsync(cancellationToken);
        if (!topics.IsSuccess)
        {
            return [];
        }

        return topics.Value
            .Select(topic => new SearchEntry(
                ServiceKey,
                topic.Name,
                topic.Name,
                $"/services/sns/{Uri.EscapeDataString(topic.TopicArn)}"))
            .ToList();
    }
}
