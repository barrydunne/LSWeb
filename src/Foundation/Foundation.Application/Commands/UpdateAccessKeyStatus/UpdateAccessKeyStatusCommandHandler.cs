using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Iam;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UpdateAccessKeyStatus;

internal sealed partial class UpdateAccessKeyStatusCommandHandler : ICommandHandler<UpdateAccessKeyStatusCommand>
{
    private const string OperationName = "iam-update-access-key-status";

    private readonly IIamClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public UpdateAccessKeyStatusCommandHandler(
        IIamClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<UpdateAccessKeyStatusCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateAccessKeyStatusCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.AccessKeyId, request.Status);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Updating access key {request.AccessKeyId} to {request.Status}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.UpdateAccessKeyStatusAsync(
            request.UserName, request.AccessKeyId, request.Status, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to update access key {request.AccessKeyId}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Updated access key {request.AccessKeyId} to {request.Status}.";
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

    [LoggerMessage(LogLevel.Trace, "Updating IAM access key {AccessKeyId} to status {Status}.")]
    private partial void LogHandling(string accessKeyId, string status);

    [LoggerMessage(LogLevel.Trace, "IAM update access key status handled.")]
    private partial void LogHandled();
}
