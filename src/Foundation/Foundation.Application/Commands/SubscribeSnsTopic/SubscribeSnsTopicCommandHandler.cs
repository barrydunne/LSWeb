using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Sns;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.SubscribeSnsTopic;

internal sealed partial class SubscribeSnsTopicCommandHandler : ICommandHandler<SubscribeSnsTopicCommand>
{
    private const string OperationName = "sns-subscribe";

    private readonly ISnsClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public SubscribeSnsTopicCommandHandler(
        ISnsClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<SubscribeSnsTopicCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(SubscribeSnsTopicCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.TopicArn, request.Protocol);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Subscribing {request.Endpoint} ({request.Protocol}) to {request.TopicArn}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.SubscribeAsync(
            request.TopicArn, request.Protocol, request.Endpoint, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to subscribe {request.Endpoint} to {request.TopicArn}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Subscribed {request.Endpoint} ({request.Protocol}) to {request.TopicArn}.";
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

    [LoggerMessage(LogLevel.Trace, "Subscribing endpoint to SNS topic {TopicArn} using protocol {Protocol}.")]
    private partial void LogHandling(string topicArn, string protocol);

    [LoggerMessage(LogLevel.Trace, "SNS subscribe handled.")]
    private partial void LogHandled();
}
