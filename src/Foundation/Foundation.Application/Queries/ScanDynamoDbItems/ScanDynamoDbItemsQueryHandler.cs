using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.DynamoDb;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ScanDynamoDbItems;

internal sealed partial class ScanDynamoDbItemsQueryHandler
    : IQueryHandler<ScanDynamoDbItemsQuery, ScanDynamoDbItemsQueryResult>
{
    private readonly IDynamoDbClient _client;
    private readonly ILogger _logger;

    public ScanDynamoDbItemsQueryHandler(IDynamoDbClient client, ILogger<ScanDynamoDbItemsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ScanDynamoDbItemsQueryResult>> Handle(
        ScanDynamoDbItemsQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.TableName, request.Limit);
        var page = await _client.ScanItemsAsync(request.TableName, request.Limit, cancellationToken);
        LogHandled(request.TableName, page.IsSuccess);

        if (!page.IsSuccess)
        {
            Result<ScanDynamoDbItemsQueryResult> failure = page.Error!.Value;
            return failure;
        }

        return new ScanDynamoDbItemsQueryResult(page.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Scanning DynamoDB table {TableName} for up to {Limit} items.")]
    private partial void LogHandling(string tableName, int limit);

    [LoggerMessage(LogLevel.Trace, "DynamoDB table {TableName} scan handled. Success: {Success}")]
    private partial void LogHandled(string tableName, bool success);
}
