using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.S3;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.PresignS3Object;

internal sealed partial class PresignS3ObjectQueryHandler : IQueryHandler<PresignS3ObjectQuery, PresignS3ObjectQueryResult>
{
    private const int DefaultExpirySeconds = 3600;
    private const int MinExpirySeconds = 1;
    private const int MaxExpirySeconds = 604800;

    private readonly IS3Client _client;
    private readonly IPresignedUrlRewriter _urlRewriter;
    private readonly ILogger _logger;

    public PresignS3ObjectQueryHandler(IS3Client client, IPresignedUrlRewriter urlRewriter, ILogger<PresignS3ObjectQueryHandler> logger)
    {
        _client = client;
        _urlRewriter = urlRewriter;
        _logger = logger;
    }

    public async Task<Result<PresignS3ObjectQueryResult>> Handle(PresignS3ObjectQuery request, CancellationToken cancellationToken)
    {
        var expirySeconds = ClampExpiry(request.ExpirySeconds);
        LogHandling(request.BucketName, request.Key, expirySeconds);
        var url = await _client.GeneratePresignedUrlAsync(
            request.BucketName, request.Key, TimeSpan.FromSeconds(expirySeconds), cancellationToken);
        LogHandled(url.IsSuccess);

        if (!url.IsSuccess)
        {
            Result<PresignS3ObjectQueryResult> failure = url.Error!.Value;
            return failure;
        }

        return new PresignS3ObjectQueryResult(_urlRewriter.Rewrite(url.Value), expirySeconds);
    }

    private static int ClampExpiry(int requested)
    {
        if (requested <= 0)
            return DefaultExpirySeconds;

        return Math.Clamp(requested, MinExpirySeconds, MaxExpirySeconds);
    }

    [LoggerMessage(LogLevel.Trace, "Generating presigned URL for S3 object {Key} in {BucketName} valid for {ExpirySeconds}s.")]
    private partial void LogHandling(string bucketName, string key, int expirySeconds);

    [LoggerMessage(LogLevel.Trace, "S3 presigned URL generation handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
