using Foundation.Application.CloudWatchLogs;
using Foundation.Domain.Search;

namespace Foundation.Infrastructure.Search;

/// <summary>
/// Contributes CloudWatch log groups to the global search index. Failures are swallowed and reported
/// as an empty list so a backend that is unavailable or unsupported cannot abort a full index
/// rebuild.
/// </summary>
internal sealed class CloudWatchLogsResourceSource : IResourceSource
{
    private readonly ICloudWatchLogsClient _client;

    public CloudWatchLogsResourceSource(ICloudWatchLogsClient client)
        => _client = client;

    /// <inheritdoc />
    public string ServiceKey => "cloudwatch-logs";

    /// <inheritdoc />
    public async Task<IReadOnlyList<SearchEntry>> ListAsync(CancellationToken cancellationToken)
    {
        var groups = await _client.ListLogGroupsAsync(cancellationToken);
        if (!groups.IsSuccess)
        {
            return [];
        }

        return groups.Value
            .Select(group => new SearchEntry(
                ServiceKey,
                group.Name,
                group.Name,
                $"/services/cloudwatch-logs/{Uri.EscapeDataString(group.Name)}"))
            .ToList();
    }
}
