using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.DynamoDb;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.ExecuteDynamoDbTransaction;

internal sealed partial class ExecuteDynamoDbTransactionCommandHandler
    : ICommandHandler<ExecuteDynamoDbTransactionCommand>
{
    private const string OperationName = "dynamodb-transaction-write";

    private readonly IDynamoDbClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public ExecuteDynamoDbTransactionCommandHandler(
        IDynamoDbClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<ExecuteDynamoDbTransactionCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(
        ExecuteDynamoDbTransactionCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.Actions.Count);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Submitting transaction with {request.Actions.Count} action(s).", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.ExecuteTransactionWriteAsync(request.Actions, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Transaction failed: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Transaction with {request.Actions.Count} action(s) committed.";
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

    [LoggerMessage(LogLevel.Trace, "Submitting DynamoDB transaction with {Count} action(s).")]
    private partial void LogHandling(int count);

    [LoggerMessage(LogLevel.Trace, "DynamoDB transaction handled.")]
    private partial void LogHandled();
}
