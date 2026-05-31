using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Iam;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteGroupInlinePolicy;

internal sealed partial class DeleteGroupInlinePolicyCommandHandler : ICommandHandler<DeleteGroupInlinePolicyCommand>
{
    private const string OperationName = "iam-delete-group-inline-policy";

    private readonly IIamClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public DeleteGroupInlinePolicyCommandHandler(
        IIamClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<DeleteGroupInlinePolicyCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteGroupInlinePolicyCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.GroupName, request.PolicyName);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Deleting inline policy {request.PolicyName} from {request.GroupName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.DeleteGroupInlinePolicyAsync(
            request.GroupName, request.PolicyName, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to delete inline policy {request.PolicyName} from {request.GroupName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Deleted inline policy {request.PolicyName} from {request.GroupName}.";
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

    [LoggerMessage(LogLevel.Trace, "Deleting inline policy {PolicyName} from IAM group {GroupName}.")]
    private partial void LogHandling(string groupName, string policyName);

    [LoggerMessage(LogLevel.Trace, "IAM delete group inline policy handled.")]
    private partial void LogHandled();
}
