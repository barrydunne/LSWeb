using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.DynamoDb;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetDynamoDbItem;

internal sealed partial class GetDynamoDbItemQueryHandler
    : IQueryHandler<GetDynamoDbItemQuery, GetDynamoDbItemQueryResult>
{
    private readonly IDynamoDbClient _client;
    private readonly ILogger _logger;

    public GetDynamoDbItemQueryHandler(IDynamoDbClient client, ILogger<GetDynamoDbItemQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetDynamoDbItemQueryResult>> Handle(
        GetDynamoDbItemQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.TableName);
        var item = await _client.GetItemAsync(request.TableName, request.KeyJson, cancellationToken);
        LogHandled(request.TableName, item.IsSuccess);

        if (!item.IsSuccess)
        {
            Result<GetDynamoDbItemQueryResult> failure = item.Error!.Value;
            return failure;
        }

        return new GetDynamoDbItemQueryResult(item.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Reading DynamoDB item from table {TableName}.")]
    private partial void LogHandling(string tableName);

    [LoggerMessage(LogLevel.Trace, "DynamoDB item read from table {TableName} handled. Success: {Success}")]
    private partial void LogHandled(string tableName, bool success);
}
