using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.S3;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.DownloadS3Object;

internal sealed partial class DownloadS3ObjectQueryHandler : IQueryHandler<DownloadS3ObjectQuery, DownloadS3ObjectQueryResult>
{
    private readonly IS3Client _client;
    private readonly ILogger _logger;

    public DownloadS3ObjectQueryHandler(IS3Client client, ILogger<DownloadS3ObjectQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<DownloadS3ObjectQueryResult>> Handle(DownloadS3ObjectQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.BucketName, request.Key);
        var content = await _client.DownloadObjectAsync(request.BucketName, request.Key, cancellationToken);
        LogHandled(content.IsSuccess);

        if (!content.IsSuccess)
        {
            Result<DownloadS3ObjectQueryResult> failure = content.Error!.Value;
            return failure;
        }

        return new DownloadS3ObjectQueryResult(
            content.Value.Content, content.Value.ContentType, FileNameFromKey(request.Key));
    }

    private static string FileNameFromKey(string key)
    {
        var segments = key.Split('/');
        var name = segments[^1];
        return string.IsNullOrEmpty(name) ? key : name;
    }

    [LoggerMessage(LogLevel.Trace, "Downloading S3 object {Key} from {BucketName}.")]
    private partial void LogHandling(string bucketName, string key);

    [LoggerMessage(LogLevel.Trace, "S3 object download handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
