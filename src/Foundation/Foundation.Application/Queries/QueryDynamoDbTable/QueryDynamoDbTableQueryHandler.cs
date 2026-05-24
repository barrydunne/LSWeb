using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.DynamoDb;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.QueryDynamoDbTable;

internal sealed partial class QueryDynamoDbTableQueryHandler
    : IQueryHandler<QueryDynamoDbTableQuery, QueryDynamoDbTableQueryResult>
{
    private readonly IDynamoDbClient _client;
    private readonly ILogger _logger;

    public QueryDynamoDbTableQueryHandler(IDynamoDbClient client, ILogger<QueryDynamoDbTableQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<QueryDynamoDbTableQueryResult>> Handle(
        QueryDynamoDbTableQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.Request.TableName, request.Request.Scan, request.Request.Limit);
        var page = await _client.QueryTableAsync(request.Request, cancellationToken);
        LogHandled(request.Request.TableName, page.IsSuccess);

        if (!page.IsSuccess)
        {
            Result<QueryDynamoDbTableQueryResult> failure = page.Error!.Value;
            return failure;
        }

        return new QueryDynamoDbTableQueryResult(page.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Querying DynamoDB table {TableName} (scan: {Scan}) for up to {Limit} items.")]
    private partial void LogHandling(string tableName, bool scan, int limit);

    [LoggerMessage(LogLevel.Trace, "DynamoDB table {TableName} query handled. Success: {Success}")]
    private partial void LogHandled(string tableName, bool success);
}
