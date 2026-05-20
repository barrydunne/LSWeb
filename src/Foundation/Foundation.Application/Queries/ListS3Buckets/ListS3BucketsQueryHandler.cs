using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.S3;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListS3Buckets;

internal sealed partial class ListS3BucketsQueryHandler : IQueryHandler<ListS3BucketsQuery, ListS3BucketsQueryResult>
{
    private readonly IS3Client _client;
    private readonly ILogger _logger;

    public ListS3BucketsQueryHandler(IS3Client client, ILogger<ListS3BucketsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListS3BucketsQueryResult>> Handle(ListS3BucketsQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var buckets = await _client.ListBucketsAsync(cancellationToken);
        LogHandled(buckets.IsSuccess);

        if (!buckets.IsSuccess)
        {
            Result<ListS3BucketsQueryResult> failure = buckets.Error!.Value;
            return failure;
        }

        return new ListS3BucketsQueryResult(buckets.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing S3 buckets.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "S3 bucket listing handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
