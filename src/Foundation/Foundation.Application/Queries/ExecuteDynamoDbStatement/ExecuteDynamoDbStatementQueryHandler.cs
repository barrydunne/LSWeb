using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.DynamoDb;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ExecuteDynamoDbStatement;

internal sealed partial class ExecuteDynamoDbStatementQueryHandler
    : IQueryHandler<ExecuteDynamoDbStatementQuery, ExecuteDynamoDbStatementQueryResult>
{
    private readonly IDynamoDbClient _client;
    private readonly ILogger _logger;

    public ExecuteDynamoDbStatementQueryHandler(
        IDynamoDbClient client, ILogger<ExecuteDynamoDbStatementQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ExecuteDynamoDbStatementQueryResult>> Handle(
        ExecuteDynamoDbStatementQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.Request.Limit);
        var result = await _client.ExecuteStatementAsync(request.Request, cancellationToken);
        LogHandled(result.IsSuccess);

        if (!result.IsSuccess)
        {
            Result<ExecuteDynamoDbStatementQueryResult> failure = result.Error!.Value;
            return failure;
        }

        return new ExecuteDynamoDbStatementQueryResult(result.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Executing DynamoDB PartiQL statement for up to {Limit} items.")]
    private partial void LogHandling(int limit);

    [LoggerMessage(LogLevel.Trace, "DynamoDB PartiQL statement handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
