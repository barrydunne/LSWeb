using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Iam;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteRoleInlinePolicy;

internal sealed partial class DeleteRoleInlinePolicyCommandHandler : ICommandHandler<DeleteRoleInlinePolicyCommand>
{
    private const string OperationName = "iam-delete-role-inline-policy";

    private readonly IIamClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public DeleteRoleInlinePolicyCommandHandler(
        IIamClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<DeleteRoleInlinePolicyCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteRoleInlinePolicyCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.RoleName, request.PolicyName);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Removing {request.PolicyName} from {request.RoleName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.DeleteRoleInlinePolicyAsync(request.RoleName, request.PolicyName, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to remove {request.PolicyName} from {request.RoleName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Removed {request.PolicyName} from {request.RoleName}.";
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

    [LoggerMessage(LogLevel.Trace, "Removing inline policy {PolicyName} from IAM role {RoleName}.")]
    private partial void LogHandling(string roleName, string policyName);

    [LoggerMessage(LogLevel.Trace, "IAM delete role inline policy handled.")]
    private partial void LogHandled();
}
