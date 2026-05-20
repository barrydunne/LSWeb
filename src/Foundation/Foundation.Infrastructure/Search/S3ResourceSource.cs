using Foundation.Application.S3;
using Foundation.Domain.Search;

namespace Foundation.Infrastructure.Search;

/// <summary>
/// Contributes S3 buckets to the global search index. Failures are swallowed and reported as an
/// empty list so an S3 backend that is unavailable or unsupported cannot abort a full index
/// rebuild.
/// </summary>
internal sealed class S3ResourceSource : IResourceSource
{
    private readonly IS3Client _client;

    public S3ResourceSource(IS3Client client)
        => _client = client;

    /// <inheritdoc />
    public string ServiceKey => "s3";

    /// <inheritdoc />
    public async Task<IReadOnlyList<SearchEntry>> ListAsync(CancellationToken cancellationToken)
    {
        var buckets = await _client.ListBucketsAsync(cancellationToken);
        if (!buckets.IsSuccess)
        {
            return [];
        }

        return buckets.Value
            .Select(bucket => new SearchEntry(
                ServiceKey,
                bucket.Name,
                bucket.Name,
                $"/services/s3/{bucket.Name}"))
            .ToList();
    }
}
