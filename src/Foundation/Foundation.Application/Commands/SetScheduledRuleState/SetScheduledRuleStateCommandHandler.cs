using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.EventBridge;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.SetScheduledRuleState;

internal sealed partial class SetScheduledRuleStateCommandHandler : ICommandHandler<SetScheduledRuleStateCommand>
{
    private const string OperationName = "eventbridge-set-rule-state";

    private readonly IEventBridgeClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public SetScheduledRuleStateCommandHandler(
        IEventBridgeClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<SetScheduledRuleStateCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(SetScheduledRuleStateCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.Name, request.State);
        var operationId = Guid.NewGuid().ToString();
        var enabling = request.State == "ENABLED";
        var verb = enabling ? "Enabling" : "Disabling";

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"{verb} scheduled rule {request.Name}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = enabling
            ? await _client.EnableRuleAsync(request.Name, request.EventBusName, cancellationToken)
            : await _client.DisableRuleAsync(request.Name, request.EventBusName, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to set scheduled rule {request.Name} state: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"{(enabling ? "Enabled" : "Disabled")} scheduled rule {request.Name}.";
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

    [LoggerMessage(LogLevel.Trace, "Setting scheduled rule {Name} state to {State}.")]
    private partial void LogHandling(string name, string state);

    [LoggerMessage(LogLevel.Trace, "Scheduled rule state change handled.")]
    private partial void LogHandled();
}
