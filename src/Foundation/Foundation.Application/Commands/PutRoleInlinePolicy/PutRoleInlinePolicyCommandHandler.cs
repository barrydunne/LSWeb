using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Iam;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.PutRoleInlinePolicy;

internal sealed partial class PutRoleInlinePolicyCommandHandler : ICommandHandler<PutRoleInlinePolicyCommand>
{
    private const string OperationName = "iam-put-role-inline-policy";

    private readonly IIamClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public PutRoleInlinePolicyCommandHandler(
        IIamClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<PutRoleInlinePolicyCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(PutRoleInlinePolicyCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.RoleName, request.PolicyName);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Applying {request.PolicyName} to {request.RoleName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.PutRoleInlinePolicyAsync(
            request.RoleName, request.PolicyName, request.PolicyDocument, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to apply {request.PolicyName} to {request.RoleName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Applied {request.PolicyName} to {request.RoleName}.";
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

    [LoggerMessage(LogLevel.Trace, "Applying inline policy {PolicyName} to IAM role {RoleName}.")]
    private partial void LogHandling(string roleName, string policyName);

    [LoggerMessage(LogLevel.Trace, "IAM put role inline policy handled.")]
    private partial void LogHandled();
}
