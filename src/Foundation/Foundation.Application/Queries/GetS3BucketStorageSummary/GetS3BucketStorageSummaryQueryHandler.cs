using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.S3;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetS3BucketStorageSummary;

internal sealed partial class GetS3BucketStorageSummaryQueryHandler
    : IQueryHandler<GetS3BucketStorageSummaryQuery, GetS3BucketStorageSummaryQueryResult>
{
    private readonly IS3Client _client;
    private readonly ILogger _logger;

    public GetS3BucketStorageSummaryQueryHandler(IS3Client client, ILogger<GetS3BucketStorageSummaryQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetS3BucketStorageSummaryQueryResult>> Handle(
        GetS3BucketStorageSummaryQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.BucketName);
        var summary = await _client.GetBucketStorageSummaryAsync(request.BucketName, cancellationToken);
        LogHandled(summary.IsSuccess);

        if (!summary.IsSuccess)
        {
            Result<GetS3BucketStorageSummaryQueryResult> failure = summary.Error!.Value;
            return failure;
        }

        var value = summary.Value;
        return new GetS3BucketStorageSummaryQueryResult(value.ObjectCount, value.TotalSizeBytes);
    }

    [LoggerMessage(LogLevel.Trace, "Reading S3 storage summary for {BucketName}.")]
    private partial void LogHandling(string bucketName);

    [LoggerMessage(LogLevel.Trace, "S3 storage summary read handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
