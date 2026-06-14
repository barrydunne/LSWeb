using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.S3;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.SetS3BucketVersioning;

internal sealed partial class SetS3BucketVersioningCommandHandler : ICommandHandler<SetS3BucketVersioningCommand>
{
    private const string OperationName = "s3-bucket-versioning-set";

    private readonly IS3Client _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public SetS3BucketVersioningCommandHandler(
        IS3Client client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<SetS3BucketVersioningCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(SetS3BucketVersioningCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.BucketName, request.Enabled);
        var operationId = Guid.NewGuid().ToString();
        var verb = request.Enabled ? "Enabling" : "Suspending";

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"{verb} versioning on {request.BucketName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.SetBucketVersioningAsync(request.BucketName, request.Enabled, cancellationToken);
        if (!result.IsSuccess)
        {
            var pastVerb = request.Enabled ? "enable" : "suspend";
            var failure = $"Failed to {pastVerb} versioning on {request.BucketName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var pastTense = request.Enabled ? "Enabled" : "Suspended";
        var message = $"{pastTense} versioning on {request.BucketName}.";
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

    [LoggerMessage(LogLevel.Trace, "Setting S3 bucket versioning on {BucketName} enabled: {Enabled}.")]
    private partial void LogHandling(string bucketName, bool enabled);

    [LoggerMessage(LogLevel.Trace, "S3 bucket versioning change handled.")]
    private partial void LogHandled();
}
