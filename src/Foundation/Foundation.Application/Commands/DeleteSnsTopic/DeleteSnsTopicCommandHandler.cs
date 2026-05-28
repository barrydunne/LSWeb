using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Search;
using Foundation.Application.Sns;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteSnsTopic;

internal sealed partial class DeleteSnsTopicCommandHandler : ICommandHandler<DeleteSnsTopicCommand>
{
    private const string OperationName = "sns-delete-topic";

    private readonly ISnsClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public DeleteSnsTopicCommandHandler(
        ISnsClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<DeleteSnsTopicCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteSnsTopicCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.TopicArn);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Deleting {request.TopicArn}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.DeleteTopicAsync(request.TopicArn, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to delete {request.TopicArn}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Deleted {request.TopicArn}.";
        await PublishOutcomeAsync(operationId, OperationState.Succeeded, message, cancellationToken);
        _searchRefresh.RequestRefresh();

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

    [LoggerMessage(LogLevel.Trace, "Deleting SNS topic {TopicArn}.")]
    private partial void LogHandling(string topicArn);

    [LoggerMessage(LogLevel.Trace, "SNS topic delete handled.")]
    private partial void LogHandled();
}
