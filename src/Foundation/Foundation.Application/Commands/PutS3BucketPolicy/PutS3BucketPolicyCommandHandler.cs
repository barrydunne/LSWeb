using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.S3;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.PutS3BucketPolicy;

internal sealed partial class PutS3BucketPolicyCommandHandler : ICommandHandler<PutS3BucketPolicyCommand>
{
    private const string OperationName = "s3-bucket-policy-put";

    private readonly IS3Client _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public PutS3BucketPolicyCommandHandler(
        IS3Client client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<PutS3BucketPolicyCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(PutS3BucketPolicyCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.BucketName);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Applying policy to {request.BucketName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.PutBucketPolicyAsync(request.BucketName, request.Policy, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to apply policy to {request.BucketName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Applied policy to {request.BucketName}.";
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

    [LoggerMessage(LogLevel.Trace, "Applying S3 bucket policy to {BucketName}.")]
    private partial void LogHandling(string bucketName);

    [LoggerMessage(LogLevel.Trace, "S3 bucket policy apply handled.")]
    private partial void LogHandled();
}
