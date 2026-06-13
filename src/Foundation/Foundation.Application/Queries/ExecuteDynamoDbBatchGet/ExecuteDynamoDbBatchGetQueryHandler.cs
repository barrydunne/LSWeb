using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.DynamoDb;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ExecuteDynamoDbBatchGet;

internal sealed partial class ExecuteDynamoDbBatchGetQueryHandler
    : IQueryHandler<ExecuteDynamoDbBatchGetQuery, ExecuteDynamoDbBatchGetQueryResult>
{
    private readonly IDynamoDbClient _client;
    private readonly ILogger _logger;

    public ExecuteDynamoDbBatchGetQueryHandler(
        IDynamoDbClient client, ILogger<ExecuteDynamoDbBatchGetQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ExecuteDynamoDbBatchGetQueryResult>> Handle(
        ExecuteDynamoDbBatchGetQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.Keys.Count);
        var result = await _client.ExecuteBatchGetAsync(request.Keys, cancellationToken);
        LogHandled(result.IsSuccess);

        if (!result.IsSuccess)
        {
            Result<ExecuteDynamoDbBatchGetQueryResult> failure = result.Error!.Value;
            return failure;
        }

        return new ExecuteDynamoDbBatchGetQueryResult(result.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Executing DynamoDB batch get with {Count} key(s).")]
    private partial void LogHandling(int count);

    [LoggerMessage(LogLevel.Trace, "DynamoDB batch get handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
