using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Sns;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.SetSnsSubscriptionFilterPolicy;

internal sealed partial class SetSnsSubscriptionFilterPolicyCommandHandler
    : ICommandHandler<SetSnsSubscriptionFilterPolicyCommand>
{
    private const string OperationName = "sns-set-filter-policy";

    private readonly ISnsClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public SetSnsSubscriptionFilterPolicyCommandHandler(
        ISnsClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<SetSnsSubscriptionFilterPolicyCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(
        SetSnsSubscriptionFilterPolicyCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.SubscriptionArn);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Setting the filter policy for {request.SubscriptionArn}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.SetSubscriptionFilterPolicyAsync(
            request.SubscriptionArn,
            request.FilterPolicy,
            cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to set the filter policy for {request.SubscriptionArn}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Set the filter policy for {request.SubscriptionArn}.";
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

    [LoggerMessage(LogLevel.Trace, "Setting the filter policy for SNS subscription {SubscriptionArn}.")]
    private partial void LogHandling(string subscriptionArn);

    [LoggerMessage(LogLevel.Trace, "SNS set filter policy handled.")]
    private partial void LogHandled();
}
