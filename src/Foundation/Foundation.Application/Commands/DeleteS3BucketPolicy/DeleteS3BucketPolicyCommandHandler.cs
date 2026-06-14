using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.S3;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteS3BucketPolicy;

internal sealed partial class DeleteS3BucketPolicyCommandHandler : ICommandHandler<DeleteS3BucketPolicyCommand>
{
    private const string OperationName = "s3-bucket-policy-delete";

    private readonly IS3Client _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public DeleteS3BucketPolicyCommandHandler(
        IS3Client client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<DeleteS3BucketPolicyCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteS3BucketPolicyCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.BucketName);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Removing policy from {request.BucketName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.DeleteBucketPolicyAsync(request.BucketName, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to remove policy from {request.BucketName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Removed policy from {request.BucketName}.";
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

    [LoggerMessage(LogLevel.Trace, "Removing S3 bucket policy from {BucketName}.")]
    private partial void LogHandling(string bucketName);

    [LoggerMessage(LogLevel.Trace, "S3 bucket policy removal handled.")]
    private partial void LogHandled();
}
