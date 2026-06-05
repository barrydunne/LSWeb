using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.EventBridge;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.RemoveScheduledRuleTargets;

internal sealed partial class RemoveScheduledRuleTargetsCommandHandler : ICommandHandler<RemoveScheduledRuleTargetsCommand>
{
    private const string OperationName = "eventbridge-remove-targets";

    private readonly IEventBridgeClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public RemoveScheduledRuleTargetsCommandHandler(
        IEventBridgeClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<RemoveScheduledRuleTargetsCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(RemoveScheduledRuleTargetsCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.RuleName, request.TargetIds.Count);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Removing {request.TargetIds.Count} target(s) from scheduled rule {request.RuleName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.RemoveTargetsAsync(
            request.RuleName, request.EventBusName, request.TargetIds, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to remove targets from scheduled rule {request.RuleName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Removed {request.TargetIds.Count} target(s) from scheduled rule {request.RuleName}.";
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

    [LoggerMessage(LogLevel.Trace, "Removing {Count} target(s) from scheduled rule {Name}.")]
    private partial void LogHandling(string name, int count);

    [LoggerMessage(LogLevel.Trace, "Scheduled rule targets removal handled.")]
    private partial void LogHandled();
}
