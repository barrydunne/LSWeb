using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.S3;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListS3ObjectVersions;

internal sealed partial class ListS3ObjectVersionsQueryHandler : IQueryHandler<ListS3ObjectVersionsQuery, ListS3ObjectVersionsQueryResult>
{
    private readonly IS3Client _client;
    private readonly ILogger _logger;

    public ListS3ObjectVersionsQueryHandler(IS3Client client, ILogger<ListS3ObjectVersionsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListS3ObjectVersionsQueryResult>> Handle(ListS3ObjectVersionsQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.BucketName);
        var versions = await _client.ListObjectVersionsAsync(request.BucketName, request.Prefix, cancellationToken);
        LogHandled(versions.IsSuccess);

        if (!versions.IsSuccess)
        {
            Result<ListS3ObjectVersionsQueryResult> failure = versions.Error!.Value;
            return failure;
        }

        return new ListS3ObjectVersionsQueryResult(versions.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing S3 object versions for {BucketName}.")]
    private partial void LogHandling(string bucketName);

    [LoggerMessage(LogLevel.Trace, "S3 object version list handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
