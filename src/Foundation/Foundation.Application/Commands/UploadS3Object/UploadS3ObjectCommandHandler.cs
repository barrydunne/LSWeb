using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.S3;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UploadS3Object;

internal sealed partial class UploadS3ObjectCommandHandler : ICommandHandler<UploadS3ObjectCommand>
{
    private const string OperationName = "s3-upload-object";

    private readonly IS3Client _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public UploadS3ObjectCommandHandler(
        IS3Client client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<UploadS3ObjectCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(UploadS3ObjectCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.BucketName, request.Key);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Uploading {request.Key} to {request.BucketName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.UploadObjectAsync(
            request.BucketName, request.Key, request.Content, request.ContentType, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to upload {request.Key} to {request.BucketName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Uploaded {request.Key} to {request.BucketName}.";
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

    [LoggerMessage(LogLevel.Trace, "Uploading S3 object {Key} to {BucketName}.")]
    private partial void LogHandling(string bucketName, string key);

    [LoggerMessage(LogLevel.Trace, "S3 object upload handled.")]
    private partial void LogHandled();
}
