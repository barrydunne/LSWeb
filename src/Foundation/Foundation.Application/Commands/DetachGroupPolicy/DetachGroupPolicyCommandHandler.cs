using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Iam;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DetachGroupPolicy;

internal sealed partial class DetachGroupPolicyCommandHandler : ICommandHandler<DetachGroupPolicyCommand>
{
    private const string OperationName = "iam-detach-group-policy";

    private readonly IIamClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public DetachGroupPolicyCommandHandler(
        IIamClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<DetachGroupPolicyCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(DetachGroupPolicyCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.GroupName, request.PolicyArn);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Detaching {request.PolicyArn} from {request.GroupName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.DetachGroupPolicyAsync(request.GroupName, request.PolicyArn, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to detach {request.PolicyArn} from {request.GroupName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Detached {request.PolicyArn} from {request.GroupName}.";
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

    [LoggerMessage(LogLevel.Trace, "Detaching policy {PolicyArn} from IAM group {GroupName}.")]
    private partial void LogHandling(string groupName, string policyArn);

    [LoggerMessage(LogLevel.Trace, "IAM detach group policy handled.")]
    private partial void LogHandled();
}
