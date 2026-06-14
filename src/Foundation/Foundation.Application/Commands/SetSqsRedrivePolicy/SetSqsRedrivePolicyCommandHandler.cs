using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Sqs;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.SetSqsRedrivePolicy;

internal sealed partial class SetSqsRedrivePolicyCommandHandler : ICommandHandler<SetSqsRedrivePolicyCommand>
{
    private const string OperationName = "sqs-set-redrive-policy";

    private readonly ISqsClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public SetSqsRedrivePolicyCommandHandler(
        ISqsClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<SetSqsRedrivePolicyCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(SetSqsRedrivePolicyCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.QueueName, request.MaxReceiveCount);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Configuring the dead-letter queue for {request.QueueName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.SetRedrivePolicyAsync(
            request.QueueName, request.DeadLetterTargetArn, request.MaxReceiveCount, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to configure the dead-letter queue for {request.QueueName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Configured the dead-letter queue for {request.QueueName}.";
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

    [LoggerMessage(LogLevel.Trace, "Configuring the SQS redrive policy for {QueueName} with max receive count {MaxReceiveCount}.")]
    private partial void LogHandling(string queueName, int maxReceiveCount);

    [LoggerMessage(LogLevel.Trace, "SQS set redrive policy handled.")]
    private partial void LogHandled();
}
