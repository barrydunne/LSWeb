using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.S3;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteS3ObjectVersion;

internal sealed partial class DeleteS3ObjectVersionCommandHandler : ICommandHandler<DeleteS3ObjectVersionCommand>
{
    private const string OperationName = "s3-object-version-delete";

    private readonly IS3Client _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public DeleteS3ObjectVersionCommandHandler(
        IS3Client client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<DeleteS3ObjectVersionCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteS3ObjectVersionCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.BucketName, request.Key, request.VersionId);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Deleting version {request.VersionId} of {request.Key}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.DeleteObjectVersionAsync(
            request.BucketName, request.Key, request.VersionId, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to delete version {request.VersionId} of {request.Key}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Deleted version {request.VersionId} of {request.Key}.";
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

    [LoggerMessage(LogLevel.Trace, "Deleting S3 object version {VersionId} of {Key} in {BucketName}.")]
    private partial void LogHandling(string bucketName, string key, string versionId);

    [LoggerMessage(LogLevel.Trace, "S3 object version deletion handled.")]
    private partial void LogHandled();
}
