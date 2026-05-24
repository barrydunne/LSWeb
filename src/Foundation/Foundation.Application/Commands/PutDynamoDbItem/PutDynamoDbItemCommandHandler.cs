using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.DynamoDb;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.PutDynamoDbItem;

internal sealed partial class PutDynamoDbItemCommandHandler : ICommandHandler<PutDynamoDbItemCommand>
{
    private const string OperationName = "dynamodb-put-item";

    private readonly IDynamoDbClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public PutDynamoDbItemCommandHandler(
        IDynamoDbClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<PutDynamoDbItemCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(PutDynamoDbItemCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.TableName);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Writing an item to {request.TableName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.PutItemAsync(request.TableName, request.ItemJson, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to write an item to {request.TableName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Wrote an item to {request.TableName}.";
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

    [LoggerMessage(LogLevel.Trace, "Writing an item to DynamoDB table {TableName}.")]
    private partial void LogHandling(string tableName);

    [LoggerMessage(LogLevel.Trace, "DynamoDB put item handled.")]
    private partial void LogHandled();
}
