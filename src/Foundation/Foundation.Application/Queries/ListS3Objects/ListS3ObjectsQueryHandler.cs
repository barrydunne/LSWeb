using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.S3;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListS3Objects;

internal sealed partial class ListS3ObjectsQueryHandler : IQueryHandler<ListS3ObjectsQuery, ListS3ObjectsQueryResult>
{
    private readonly IS3Client _client;
    private readonly ILogger _logger;

    public ListS3ObjectsQueryHandler(IS3Client client, ILogger<ListS3ObjectsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListS3ObjectsQueryResult>> Handle(ListS3ObjectsQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.BucketName, request.Prefix);
        var listing = await _client.ListObjectsAsync(request.BucketName, request.Prefix, cancellationToken);
        LogHandled(listing.IsSuccess);

        if (!listing.IsSuccess)
        {
            Result<ListS3ObjectsQueryResult> failure = listing.Error!.Value;
            return failure;
        }

        return new ListS3ObjectsQueryResult(listing.Value.Prefixes, listing.Value.Objects);
    }

    [LoggerMessage(LogLevel.Trace, "Listing S3 objects in {BucketName} under '{Prefix}'.")]
    private partial void LogHandling(string bucketName, string prefix);

    [LoggerMessage(LogLevel.Trace, "S3 object listing handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
