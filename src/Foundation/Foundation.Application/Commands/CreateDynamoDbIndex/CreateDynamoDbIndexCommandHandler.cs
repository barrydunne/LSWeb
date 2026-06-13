using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.DynamoDb;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.DynamoDb;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateDynamoDbIndex;

internal sealed partial class CreateDynamoDbIndexCommandHandler : ICommandHandler<CreateDynamoDbIndexCommand>
{
    private const string OperationName = "dynamodb-create-index";

    private readonly IDynamoDbClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public CreateDynamoDbIndexCommandHandler(
        IDynamoDbClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<CreateDynamoDbIndexCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(CreateDynamoDbIndexCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.TableName, request.IndexName);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Creating index {request.IndexName} on {request.TableName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var specification = new DynamoDbIndexSpecification(
            request.TableName,
            request.IndexName,
            request.PartitionKeyName,
            request.PartitionKeyType,
            request.SortKeyName,
            request.SortKeyType,
            request.ProjectionType);

        var result = await _client.CreateGlobalSecondaryIndexAsync(specification, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to create index {request.IndexName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Created index {request.IndexName} on {request.TableName}.";
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

    [LoggerMessage(LogLevel.Trace, "Creating DynamoDB index {IndexName} on {TableName}.")]
    private partial void LogHandling(string tableName, string indexName);

    [LoggerMessage(LogLevel.Trace, "DynamoDB index create handled.")]
    private partial void LogHandled();
}
