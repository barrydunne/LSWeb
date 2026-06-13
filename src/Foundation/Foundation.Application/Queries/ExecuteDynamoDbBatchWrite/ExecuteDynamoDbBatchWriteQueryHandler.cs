using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.DynamoDb;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ExecuteDynamoDbBatchWrite;

internal sealed partial class ExecuteDynamoDbBatchWriteQueryHandler
    : IQueryHandler<ExecuteDynamoDbBatchWriteQuery, ExecuteDynamoDbBatchWriteQueryResult>
{
    private readonly IDynamoDbClient _client;
    private readonly ILogger _logger;

    public ExecuteDynamoDbBatchWriteQueryHandler(
        IDynamoDbClient client, ILogger<ExecuteDynamoDbBatchWriteQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ExecuteDynamoDbBatchWriteQueryResult>> Handle(
        ExecuteDynamoDbBatchWriteQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.Items.Count);
        var result = await _client.ExecuteBatchWriteAsync(request.Items, cancellationToken);
        LogHandled(result.IsSuccess);

        if (!result.IsSuccess)
        {
            Result<ExecuteDynamoDbBatchWriteQueryResult> failure = result.Error!.Value;
            return failure;
        }

        return new ExecuteDynamoDbBatchWriteQueryResult(result.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Executing DynamoDB batch write with {Count} request(s).")]
    private partial void LogHandling(int count);

    [LoggerMessage(LogLevel.Trace, "DynamoDB batch write handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
