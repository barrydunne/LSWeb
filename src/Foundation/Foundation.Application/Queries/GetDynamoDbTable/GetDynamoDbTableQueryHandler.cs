using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.DynamoDb;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetDynamoDbTable;

internal sealed partial class GetDynamoDbTableQueryHandler
    : IQueryHandler<GetDynamoDbTableQuery, GetDynamoDbTableQueryResult>
{
    private readonly IDynamoDbClient _client;
    private readonly ILogger _logger;

    public GetDynamoDbTableQueryHandler(IDynamoDbClient client, ILogger<GetDynamoDbTableQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetDynamoDbTableQueryResult>> Handle(
        GetDynamoDbTableQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.TableName);
        var table = await _client.DescribeTableAsync(request.TableName, cancellationToken);
        LogHandled(request.TableName, table.IsSuccess);

        if (!table.IsSuccess)
        {
            Result<GetDynamoDbTableQueryResult> failure = table.Error!.Value;
            return failure;
        }

        return new GetDynamoDbTableQueryResult(table.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Describing DynamoDB table {TableName}.")]
    private partial void LogHandling(string tableName);

    [LoggerMessage(LogLevel.Trace, "DynamoDB table {TableName} description handled. Success: {Success}")]
    private partial void LogHandled(string tableName, bool success);
}
