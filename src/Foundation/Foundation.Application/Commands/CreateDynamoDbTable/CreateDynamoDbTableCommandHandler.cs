using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.DynamoDb;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.DynamoDb;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateDynamoDbTable;

internal sealed partial class CreateDynamoDbTableCommandHandler : ICommandHandler<CreateDynamoDbTableCommand>
{
    private const string OperationName = "dynamodb-create-table";

    private readonly IDynamoDbClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public CreateDynamoDbTableCommandHandler(
        IDynamoDbClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<CreateDynamoDbTableCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(CreateDynamoDbTableCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.TableName);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Creating {request.TableName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var specification = new DynamoDbTableSpecification(
            request.TableName,
            request.PartitionKeyName,
            request.PartitionKeyType,
            request.SortKeyName,
            request.SortKeyType,
            request.BillingMode,
            request.ReadCapacityUnits,
            request.WriteCapacityUnits);

        var result = await _client.CreateTableAsync(specification, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to create {request.TableName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Created {request.TableName}.";
        await PublishOutcomeAsync(operationId, OperationState.Succeeded, message, cancellationToken);
        _searchRefresh.RequestRefresh();

        LogHandled();
        return Result.Success();
    }

    private async Task PublishOutcomeAsync(string operationId, OperationState state, string message, CancellationToken cancellationToken)
    {
        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, state, message, DateTimeOffset.UtcNow),
            cancellationToken);
        _activityLog.Append(
            new ActivityEntry(operationId, OperationName, state, message, DateTimeOffset.UtcNow));
    }

    [LoggerMessage(LogLevel.Trace, "Creating DynamoDB table {TableName}.")]
    private partial void LogHandling(string tableName);

    [LoggerMessage(LogLevel.Trace, "DynamoDB table create handled.")]
    private partial void LogHandled();
}
