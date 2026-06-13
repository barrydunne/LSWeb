using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.EventBridge;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.EventBridge;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.PutEventBridgeRule;

internal sealed partial class PutEventBridgeRuleCommandHandler : ICommandHandler<PutEventBridgeRuleCommand>
{
    private const string OperationName = "eventbridge-put-pattern-rule";

    private readonly IEventBridgeClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public PutEventBridgeRuleCommandHandler(
        IEventBridgeClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<PutEventBridgeRuleCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(PutEventBridgeRuleCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.Name);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Saving rule {request.Name}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var specification = new EventBridgeRulePatternSpecification(
            request.Name,
            request.EventPattern,
            request.State,
            request.Description,
            request.EventBusName);

        var result = await _client.PutEventPatternRuleAsync(specification, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to save rule {request.Name}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Saved rule {request.Name}.";
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

    [LoggerMessage(LogLevel.Trace, "Saving event-pattern rule {Name}.")]
    private partial void LogHandling(string name);

    [LoggerMessage(LogLevel.Trace, "Event-pattern rule save handled.")]
    private partial void LogHandled();
}
