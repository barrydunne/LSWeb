using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.DynamoDb;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteDynamoDbItem;

internal sealed partial class DeleteDynamoDbItemCommandHandler : ICommandHandler<DeleteDynamoDbItemCommand>
{
    private const string OperationName = "dynamodb-delete-item";

    private readonly IDynamoDbClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public DeleteDynamoDbItemCommandHandler(
        IDynamoDbClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<DeleteDynamoDbItemCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteDynamoDbItemCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.TableName);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Deleting an item from {request.TableName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.DeleteItemAsync(request.TableName, request.KeyJson, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to delete an item from {request.TableName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Deleted an item from {request.TableName}.";
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

    [LoggerMessage(LogLevel.Trace, "Deleting an item from DynamoDB table {TableName}.")]
    private partial void LogHandling(string tableName);

    [LoggerMessage(LogLevel.Trace, "DynamoDB delete item handled.")]
    private partial void LogHandled();
}
