using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.S3;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetS3ObjectMetadata;

internal sealed partial class GetS3ObjectMetadataQueryHandler
    : IQueryHandler<GetS3ObjectMetadataQuery, GetS3ObjectMetadataQueryResult>
{
    private readonly IS3Client _client;
    private readonly ILogger _logger;

    public GetS3ObjectMetadataQueryHandler(IS3Client client, ILogger<GetS3ObjectMetadataQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetS3ObjectMetadataQueryResult>> Handle(
        GetS3ObjectMetadataQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.BucketName, request.Key);
        var metadata = await _client.GetObjectMetadataAsync(request.BucketName, request.Key, cancellationToken);
        LogHandled(metadata.IsSuccess);

        if (!metadata.IsSuccess)
        {
            Result<GetS3ObjectMetadataQueryResult> failure = metadata.Error!.Value;
            return failure;
        }

        var value = metadata.Value;
        var userMetadata = value.UserMetadata
            .OrderBy(entry => entry.Key, StringComparer.Ordinal)
            .Select(entry => new S3MetadataEntry(entry.Key, entry.Value))
            .ToList();
        var tags = value.Tags
            .OrderBy(entry => entry.Key, StringComparer.Ordinal)
            .Select(entry => new S3MetadataEntry(entry.Key, entry.Value))
            .ToList();

        return new GetS3ObjectMetadataQueryResult(
            value.ContentType, value.ContentLength, value.LastModified, value.ETag, userMetadata, tags);
    }

    [LoggerMessage(LogLevel.Trace, "Reading S3 object metadata for {Key} from {BucketName}.")]
    private partial void LogHandling(string bucketName, string key);

    [LoggerMessage(LogLevel.Trace, "S3 object metadata read handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
