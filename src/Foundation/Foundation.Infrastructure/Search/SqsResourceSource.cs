using Foundation.Application.Sqs;
using Foundation.Domain.Search;

namespace Foundation.Infrastructure.Search;

/// <summary>
/// Contributes SQS queues to the global search index. Failures are swallowed and reported as an
/// empty list so an SQS backend that is unavailable or unsupported cannot abort a full index
/// rebuild.
/// </summary>
internal sealed class SqsResourceSource : IResourceSource
{
    private readonly ISqsClient _client;

    public SqsResourceSource(ISqsClient client)
        => _client = client;

    /// <inheritdoc />
    public string ServiceKey => "sqs";

    /// <inheritdoc />
    public async Task<IReadOnlyList<SearchEntry>> ListAsync(CancellationToken cancellationToken)
    {
        var queues = await _client.ListQueuesAsync(cancellationToken);
        if (!queues.IsSuccess)
        {
            return [];
        }

        return queues.Value
            .Select(queue => new SearchEntry(
                ServiceKey,
                queue.Name,
                queue.Name,
                $"/services/sqs/{queue.Name}"))
            .ToList();
    }
}
