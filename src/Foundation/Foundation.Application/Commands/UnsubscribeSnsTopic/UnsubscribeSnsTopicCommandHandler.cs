using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Sns;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UnsubscribeSnsTopic;

internal sealed partial class UnsubscribeSnsTopicCommandHandler : ICommandHandler<UnsubscribeSnsTopicCommand>
{
    private const string OperationName = "sns-unsubscribe";

    private readonly ISnsClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public UnsubscribeSnsTopicCommandHandler(
        ISnsClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<UnsubscribeSnsTopicCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(UnsubscribeSnsTopicCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.SubscriptionArn);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Removing subscription {request.SubscriptionArn}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.UnsubscribeAsync(request.SubscriptionArn, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to remove subscription {request.SubscriptionArn}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Removed subscription {request.SubscriptionArn}.";
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

    [LoggerMessage(LogLevel.Trace, "Removing SNS subscription {SubscriptionArn}.")]
    private partial void LogHandling(string subscriptionArn);

    [LoggerMessage(LogLevel.Trace, "SNS unsubscribe handled.")]
    private partial void LogHandled();
}
