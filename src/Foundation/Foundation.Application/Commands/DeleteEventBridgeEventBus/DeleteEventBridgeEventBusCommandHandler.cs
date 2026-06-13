using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.EventBridge;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteEventBridgeEventBus;

internal sealed partial class DeleteEventBridgeEventBusCommandHandler
    : ICommandHandler<DeleteEventBridgeEventBusCommand>
{
    private const string OperationName = "eventbridge-delete-bus";

    private readonly IEventBridgeClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public DeleteEventBridgeEventBusCommandHandler(
        IEventBridgeClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<DeleteEventBridgeEventBusCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(
        DeleteEventBridgeEventBusCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.Name);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Deleting event bus {request.Name}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.DeleteEventBusAsync(request.Name, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to delete event bus {request.Name}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Deleted event bus {request.Name}.";
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

    [LoggerMessage(LogLevel.Trace, "Deleting EventBridge event bus {Name}.")]
    private partial void LogHandling(string name);

    [LoggerMessage(LogLevel.Trace, "EventBridge event bus delete handled.")]
    private partial void LogHandled();
}
