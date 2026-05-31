using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Iam;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteAccessKey;

internal sealed partial class DeleteAccessKeyCommandHandler : ICommandHandler<DeleteAccessKeyCommand>
{
    private const string OperationName = "iam-delete-access-key";

    private readonly IIamClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public DeleteAccessKeyCommandHandler(
        IIamClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<DeleteAccessKeyCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteAccessKeyCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.AccessKeyId, request.UserName);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Deleting access key {request.AccessKeyId} from {request.UserName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.DeleteAccessKeyAsync(request.UserName, request.AccessKeyId, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to delete access key {request.AccessKeyId} from {request.UserName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Deleted access key {request.AccessKeyId} from {request.UserName}.";
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

    [LoggerMessage(LogLevel.Trace, "Deleting IAM access key {AccessKeyId} from user {UserName}.")]
    private partial void LogHandling(string accessKeyId, string userName);

    [LoggerMessage(LogLevel.Trace, "IAM delete access key handled.")]
    private partial void LogHandled();
}
