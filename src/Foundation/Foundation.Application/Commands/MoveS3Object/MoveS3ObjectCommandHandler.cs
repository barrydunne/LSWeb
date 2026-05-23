using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.S3;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.MoveS3Object;

internal sealed partial class MoveS3ObjectCommandHandler : ICommandHandler<MoveS3ObjectCommand>
{
    private const string OperationName = "s3-move-object";

    private readonly IS3Client _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public MoveS3ObjectCommandHandler(
        IS3Client client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<MoveS3ObjectCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(MoveS3ObjectCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.SourceBucketName, request.SourceKey, request.DestinationBucketName, request.DestinationKey);
        var operationId = Guid.NewGuid().ToString();

        var source = $"{request.SourceKey} in {request.SourceBucketName}";
        var destination = $"{request.DestinationKey} in {request.DestinationBucketName}";

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Moving {source} to {destination}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var copyResult = await _client.CopyObjectAsync(
            request.SourceBucketName, request.SourceKey, request.DestinationBucketName, request.DestinationKey, cancellationToken);
        if (!copyResult.IsSuccess)
        {
            var failure = $"Failed to move {source} to {destination}: {copyResult.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return copyResult.Error!.Value;
        }

        var deleteResult = await _client.DeleteObjectAsync(request.SourceBucketName, request.SourceKey, cancellationToken);
        if (!deleteResult.IsSuccess)
        {
            var failure = $"Failed to move {source} to {destination}: {deleteResult.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            _searchRefresh.RequestRefresh();
            return deleteResult.Error!.Value;
        }

        var message = $"Moved {source} to {destination}.";
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

    [LoggerMessage(LogLevel.Trace, "Moving S3 object {SourceKey} in {SourceBucketName} to {DestinationKey} in {DestinationBucketName}.")]
    private partial void LogHandling(string sourceBucketName, string sourceKey, string destinationBucketName, string destinationKey);

    [LoggerMessage(LogLevel.Trace, "S3 object move handled.")]
    private partial void LogHandled();
}
