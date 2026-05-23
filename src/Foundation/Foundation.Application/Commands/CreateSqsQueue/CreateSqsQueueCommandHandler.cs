using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Search;
using Foundation.Application.Sqs;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateSqsQueue;

internal sealed partial class CreateSqsQueueCommandHandler : ICommandHandler<CreateSqsQueueCommand>
{
    private const string OperationName = "sqs-create-queue";

    private readonly ISqsClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public CreateSqsQueueCommandHandler(
        ISqsClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<CreateSqsQueueCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(CreateSqsQueueCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.QueueName);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Creating {request.QueueName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.CreateQueueAsync(request.QueueName, request.FifoQueue, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to create {request.QueueName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Created {request.QueueName}.";
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

    [LoggerMessage(LogLevel.Trace, "Creating SQS queue {QueueName}.")]
    private partial void LogHandling(string queueName);

    [LoggerMessage(LogLevel.Trace, "SQS queue create handled.")]
    private partial void LogHandled();
}
