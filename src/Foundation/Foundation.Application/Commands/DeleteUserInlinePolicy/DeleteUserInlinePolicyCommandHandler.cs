using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Iam;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteUserInlinePolicy;

internal sealed partial class DeleteUserInlinePolicyCommandHandler : ICommandHandler<DeleteUserInlinePolicyCommand>
{
    private const string OperationName = "iam-delete-user-inline-policy";

    private readonly IIamClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public DeleteUserInlinePolicyCommandHandler(
        IIamClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<DeleteUserInlinePolicyCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteUserInlinePolicyCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.UserName, request.PolicyName);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Deleting inline policy {request.PolicyName} from {request.UserName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.DeleteUserInlinePolicyAsync(
            request.UserName, request.PolicyName, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to delete inline policy {request.PolicyName} from {request.UserName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Deleted inline policy {request.PolicyName} from {request.UserName}.";
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

    [LoggerMessage(LogLevel.Trace, "Deleting inline policy {PolicyName} from IAM user {UserName}.")]
    private partial void LogHandling(string userName, string policyName);

    [LoggerMessage(LogLevel.Trace, "IAM delete user inline policy handled.")]
    private partial void LogHandled();
}
