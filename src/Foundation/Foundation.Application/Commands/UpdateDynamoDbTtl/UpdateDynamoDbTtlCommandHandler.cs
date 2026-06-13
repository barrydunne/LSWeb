using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.DynamoDb;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UpdateDynamoDbTtl;

internal sealed partial class UpdateDynamoDbTtlCommandHandler : ICommandHandler<UpdateDynamoDbTtlCommand>
{
    private const string OperationName = "dynamodb-update-ttl";

    private readonly IDynamoDbClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public UpdateDynamoDbTtlCommandHandler(
        IDynamoDbClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<UpdateDynamoDbTtlCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateDynamoDbTtlCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.TableName, request.Enabled);
        var operationId = Guid.NewGuid().ToString();
        var verb = request.Enabled ? "Enabling" : "Disabling";

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"{verb} TTL on {request.TableName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.UpdateTimeToLiveAsync(
            request.TableName, request.Enabled, request.AttributeName, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to update TTL on {request.TableName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var past = request.Enabled ? "Enabled" : "Disabled";
        var message = $"{past} TTL on {request.TableName}.";
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

    [LoggerMessage(LogLevel.Trace, "Updating DynamoDB TTL on {TableName} (enabled {Enabled}).")]
    private partial void LogHandling(string tableName, bool enabled);

    [LoggerMessage(LogLevel.Trace, "DynamoDB TTL update handled.")]
    private partial void LogHandled();
}
