using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.S3;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.PutS3BucketNotifications;

internal sealed partial class PutS3BucketNotificationsCommandHandler : ICommandHandler<PutS3BucketNotificationsCommand>
{
    private const string OperationName = "s3-bucket-notifications-put";

    private readonly IS3Client _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public PutS3BucketNotificationsCommandHandler(
        IS3Client client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<PutS3BucketNotificationsCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(PutS3BucketNotificationsCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.BucketName, request.Notifications.Count);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Updating event notifications on {request.BucketName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.PutBucketNotificationsAsync(
            request.BucketName, request.Notifications, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to update event notifications on {request.BucketName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Updated event notifications on {request.BucketName}.";
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

    [LoggerMessage(LogLevel.Trace, "Updating S3 bucket notifications on {BucketName} with {Count} rules.")]
    private partial void LogHandling(string bucketName, int count);

    [LoggerMessage(LogLevel.Trace, "S3 bucket notifications update handled.")]
    private partial void LogHandled();
}
