using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Sqs;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.SendSqsMessage;

internal sealed partial class SendSqsMessageCommandHandler : ICommandHandler<SendSqsMessageCommand>
{
    private const string OperationName = "sqs-send-message";

    private readonly ISqsClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public SendSqsMessageCommandHandler(
        ISqsClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<SendSqsMessageCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(SendSqsMessageCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.QueueName);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Sending a message to {request.QueueName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.SendMessageAsync(
            request.QueueName,
            request.Body,
            request.MessageAttributes,
            request.MessageGroupId,
            request.MessageDeduplicationId,
            cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to send a message to {request.QueueName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Sent a message to {request.QueueName}.";
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

    [LoggerMessage(LogLevel.Trace, "Sending a message to SQS queue {QueueName}.")]
    private partial void LogHandling(string queueName);

    [LoggerMessage(LogLevel.Trace, "SQS send message handled.")]
    private partial void LogHandled();
}
