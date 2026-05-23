using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.S3;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UpdateS3ObjectTags;

internal sealed partial class UpdateS3ObjectTagsCommandHandler : ICommandHandler<UpdateS3ObjectTagsCommand>
{
    private const string OperationName = "s3-update-object-tags";

    private readonly IS3Client _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public UpdateS3ObjectTagsCommandHandler(
        IS3Client client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<UpdateS3ObjectTagsCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateS3ObjectTagsCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.BucketName, request.Key, request.Tags.Count);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Updating tags for {request.Key} in {request.BucketName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.UpdateObjectTagsAsync(request.BucketName, request.Key, request.Tags, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to update tags for {request.Key} in {request.BucketName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Updated tags for {request.Key} in {request.BucketName}.";
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

    [LoggerMessage(LogLevel.Trace, "Updating tags for S3 object {Key} in {BucketName} with {Count} tag(s).")]
    private partial void LogHandling(string bucketName, string key, int count);

    [LoggerMessage(LogLevel.Trace, "S3 object tags update handled.")]
    private partial void LogHandled();
}
