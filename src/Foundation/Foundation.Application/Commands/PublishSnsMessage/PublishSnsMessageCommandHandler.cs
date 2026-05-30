using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Sns;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.PublishSnsMessage;

internal sealed partial class PublishSnsMessageCommandHandler : ICommandHandler<PublishSnsMessageCommand>
{
    private const string OperationName = "sns-publish-message";

    private readonly ISnsClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public PublishSnsMessageCommandHandler(
        ISnsClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<PublishSnsMessageCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(PublishSnsMessageCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.TopicArn);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Publishing a message to {request.TopicArn}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.PublishAsync(
            request.TopicArn,
            request.Subject,
            request.Message,
            request.MessageAttributes,
            cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to publish a message to {request.TopicArn}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Published a message to {request.TopicArn}.";
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

    [LoggerMessage(LogLevel.Trace, "Publishing a message to SNS topic {TopicArn}.")]
    private partial void LogHandling(string topicArn);

    [LoggerMessage(LogLevel.Trace, "SNS publish message handled.")]
    private partial void LogHandled();
}
