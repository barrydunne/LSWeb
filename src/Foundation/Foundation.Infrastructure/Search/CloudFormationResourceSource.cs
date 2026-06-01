using Foundation.Application.CloudFormation;
using Foundation.Domain.Search;

namespace Foundation.Infrastructure.Search;

/// <summary>
/// Contributes CloudFormation stacks to the global search index. Failures are swallowed and
/// reported as an empty list so a backend that is unavailable or unsupported cannot abort a full
/// index rebuild.
/// </summary>
internal sealed class CloudFormationResourceSource : IResourceSource
{
    private readonly ICloudFormationClient _client;

    public CloudFormationResourceSource(ICloudFormationClient client)
        => _client = client;

    /// <inheritdoc />
    public string ServiceKey => "cloudformation";

    /// <inheritdoc />
    public async Task<IReadOnlyList<SearchEntry>> ListAsync(CancellationToken cancellationToken)
    {
        var stacks = await _client.ListStacksAsync(cancellationToken);
        if (!stacks.IsSuccess)
        {
            return [];
        }

        return stacks.Value
            .Select(stack => new SearchEntry(
                ServiceKey,
                stack.StackName,
                stack.StackName,
                $"/services/cloudformation/{Uri.EscapeDataString(stack.StackName)}"))
            .ToList();
    }
}
