using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.DynamoDb;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListDynamoDbTables;

internal sealed partial class ListDynamoDbTablesQueryHandler
    : IQueryHandler<ListDynamoDbTablesQuery, ListDynamoDbTablesQueryResult>
{
    private readonly IDynamoDbClient _client;
    private readonly ILogger _logger;

    public ListDynamoDbTablesQueryHandler(IDynamoDbClient client, ILogger<ListDynamoDbTablesQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListDynamoDbTablesQueryResult>> Handle(
        ListDynamoDbTablesQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var tables = await _client.ListTablesAsync(cancellationToken);
        LogHandled(tables.IsSuccess);

        if (!tables.IsSuccess)
        {
            Result<ListDynamoDbTablesQueryResult> failure = tables.Error!.Value;
            return failure;
        }

        return new ListDynamoDbTablesQueryResult(tables.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing DynamoDB tables.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "DynamoDB table listing handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
