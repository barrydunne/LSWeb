using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.EventBridge;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.EventBridge;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.PutEventBridgeEvent;

internal sealed partial class PutEventBridgeEventCommandHandler
    : ICommandHandler<PutEventBridgeEventCommand, EventBridgePutResult>
{
    private const string OperationName = "eventbridge-put-event";

    private readonly IEventBridgeClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public PutEventBridgeEventCommandHandler(
        IEventBridgeClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<PutEventBridgeEventCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result<EventBridgePutResult>> Handle(
        PutEventBridgeEventCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.Source);
        var operationId = Guid.NewGuid().ToString();
        var busName = string.IsNullOrWhiteSpace(request.EventBusName) ? "default" : request.EventBusName;

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Putting event {request.DetailType} from {request.Source} onto {busName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.PutEventAsync(
            request.Source, request.DetailType, request.Detail, request.EventBusName, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to put event onto {busName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        if (!result.Value.Accepted)
        {
            var rejection = $"EventBridge rejected the event on {busName}: {result.Value.ErrorMessage ?? result.Value.ErrorCode ?? "unknown error"}.";
            await PublishOutcomeAsync(operationId, OperationState.Failed, rejection, cancellationToken);
            return result.Value;
        }

        await PublishOutcomeAsync(operationId, OperationState.Succeeded,
            $"Put event {result.Value.EventId} onto {busName}.", cancellationToken);

        LogHandled();
        return result.Value;
    }

    private async Task PublishOutcomeAsync(string operationId, OperationState state, string message, CancellationToken cancellationToken)
    {
        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, state, message, DateTimeOffset.UtcNow),
            cancellationToken);
        _activityLog.Append(
            new ActivityEntry(operationId, OperationName, state, message, DateTimeOffset.UtcNow));
    }

    [LoggerMessage(LogLevel.Trace, "Putting EventBridge event from {Source}.")]
    private partial void LogHandling(string source);

    [LoggerMessage(LogLevel.Trace, "EventBridge put event handled.")]
    private partial void LogHandled();
}
