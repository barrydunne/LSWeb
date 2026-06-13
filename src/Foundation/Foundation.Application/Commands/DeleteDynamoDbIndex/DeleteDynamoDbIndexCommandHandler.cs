using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.DynamoDb;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteDynamoDbIndex;

internal sealed partial class DeleteDynamoDbIndexCommandHandler : ICommandHandler<DeleteDynamoDbIndexCommand>
{
    private const string OperationName = "dynamodb-delete-index";

    private readonly IDynamoDbClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public DeleteDynamoDbIndexCommandHandler(
        IDynamoDbClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<DeleteDynamoDbIndexCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteDynamoDbIndexCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.TableName, request.IndexName);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Deleting index {request.IndexName} from {request.TableName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.DeleteGlobalSecondaryIndexAsync(
            request.TableName, request.IndexName, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to delete index {request.IndexName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Deleted index {request.IndexName} from {request.TableName}.";
        await PublishOutcomeAsync(operationId, OperationState.Succeeded, message, cancellationToken);

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

    [LoggerMessage(LogLevel.Trace, "Deleting DynamoDB index {IndexName} from {TableName}.")]
    private partial void LogHandling(string tableName, string indexName);

    [LoggerMessage(LogLevel.Trace, "DynamoDB index delete handled.")]
    private partial void LogHandled();
}
