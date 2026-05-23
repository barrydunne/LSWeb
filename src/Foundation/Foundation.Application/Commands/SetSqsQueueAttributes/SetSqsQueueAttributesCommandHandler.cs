using System.Globalization;
using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Sqs;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.SetSqsQueueAttributes;

internal sealed partial class SetSqsQueueAttributesCommandHandler : ICommandHandler<SetSqsQueueAttributesCommand>
{
    private const string OperationName = "sqs-set-queue-attributes";

    private readonly ISqsClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public SetSqsQueueAttributesCommandHandler(
        ISqsClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<SetSqsQueueAttributesCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(SetSqsQueueAttributesCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.QueueName);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Updating attributes for {request.QueueName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var attributes = new Dictionary<string, string>
        {
            ["VisibilityTimeout"] = ToValue(request.VisibilityTimeoutSeconds),
            ["MessageRetentionPeriod"] = ToValue(request.MessageRetentionPeriodSeconds),
            ["DelaySeconds"] = ToValue(request.DelaySeconds),
            ["ReceiveMessageWaitTimeSeconds"] = ToValue(request.ReceiveMessageWaitTimeSeconds),
        };

        var result = await _client.SetQueueAttributesAsync(request.QueueName, attributes, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to update attributes for {request.QueueName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Updated attributes for {request.QueueName}.";
        await PublishOutcomeAsync(operationId, OperationState.Succeeded, message, cancellationToken);

        LogHandled();
        return Result.Success();
    }

    private static string ToValue(int value)
        => value.ToString(CultureInfo.InvariantCulture);

    private async Task PublishOutcomeAsync(string operationId, OperationState state, string message, CancellationToken cancellationToken)
    {
        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, state, message, DateTimeOffset.UtcNow),
            cancellationToken);
        _activityLog.Append(
            new ActivityEntry(operationId, OperationName, state, message, DateTimeOffset.UtcNow));
    }

    [LoggerMessage(LogLevel.Trace, "Updating attributes for SQS queue {QueueName}.")]
    private partial void LogHandling(string queueName);

    [LoggerMessage(LogLevel.Trace, "SQS set queue attributes handled.")]
    private partial void LogHandled();
}
