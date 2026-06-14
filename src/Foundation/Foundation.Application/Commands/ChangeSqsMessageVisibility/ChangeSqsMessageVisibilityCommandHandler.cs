using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Sqs;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.ChangeSqsMessageVisibility;

internal sealed partial class ChangeSqsMessageVisibilityCommandHandler
    : ICommandHandler<ChangeSqsMessageVisibilityCommand>
{
    private const string OperationName = "sqs-change-message-visibility";

    private readonly ISqsClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public ChangeSqsMessageVisibilityCommandHandler(
        ISqsClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<ChangeSqsMessageVisibilityCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(ChangeSqsMessageVisibilityCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.QueueName, request.VisibilityTimeoutSeconds);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Setting message visibility on {request.QueueName} to {request.VisibilityTimeoutSeconds}s.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.ChangeMessageVisibilityAsync(
            request.QueueName, request.ReceiptHandle, request.VisibilityTimeoutSeconds, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to set message visibility on {request.QueueName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Set message visibility on {request.QueueName} to {request.VisibilityTimeoutSeconds}s.";
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

    [LoggerMessage(LogLevel.Trace, "Setting visibility timeout on an SQS message in {QueueName} to {VisibilityTimeoutSeconds}s.")]
    private partial void LogHandling(string queueName, int visibilityTimeoutSeconds);

    [LoggerMessage(LogLevel.Trace, "SQS change message visibility handled.")]
    private partial void LogHandled();
}
