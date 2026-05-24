using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.DynamoDb;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteDynamoDbTable;

internal sealed partial class DeleteDynamoDbTableCommandHandler : ICommandHandler<DeleteDynamoDbTableCommand>
{
    private const string OperationName = "dynamodb-delete-table";

    private readonly IDynamoDbClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public DeleteDynamoDbTableCommandHandler(
        IDynamoDbClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<DeleteDynamoDbTableCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteDynamoDbTableCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.TableName);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Deleting {request.TableName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.DeleteTableAsync(request.TableName, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to delete {request.TableName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Deleted {request.TableName}.";
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

    [LoggerMessage(LogLevel.Trace, "Deleting DynamoDB table {TableName}.")]
    private partial void LogHandling(string tableName);

    [LoggerMessage(LogLevel.Trace, "DynamoDB table delete handled.")]
    private partial void LogHandled();
}
