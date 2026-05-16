using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Bulk;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.ExecuteBulkAction;

internal sealed partial class ExecuteBulkActionCommandHandler : ICommandHandler<ExecuteBulkActionCommand, BulkActionOutcome>
{
    private const string OperationName = "bulk-action";

    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public ExecuteBulkActionCommandHandler(INotificationPublisher publisher, IActivityLog activityLog, ILogger<ExecuteBulkActionCommandHandler> logger)
    {
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result<BulkActionOutcome>> Handle(ExecuteBulkActionCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.Action, request.ResourceIds.Count);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Running '{request.Action}' on {request.ResourceIds.Count} item(s).", DateTimeOffset.UtcNow),
            cancellationToken);

        var items = new List<BulkActionItemResult>(request.ResourceIds.Count);
        foreach (var resourceId in request.ResourceIds)
        {
            if (string.IsNullOrWhiteSpace(resourceId))
                items.Add(new BulkActionItemResult(resourceId ?? string.Empty, false, "Resource id is required."));
            else
                items.Add(new BulkActionItemResult(resourceId, true, null));
        }

        var outcome = new BulkActionOutcome(operationId, request.Action, items);
        var message = $"'{request.Action}' completed: {outcome.SucceededCount} succeeded, {outcome.FailedCount} failed.";

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, outcome.OverallState, message, DateTimeOffset.UtcNow),
            cancellationToken);

        _activityLog.Append(
            new ActivityEntry(operationId, OperationName, outcome.OverallState, message, DateTimeOffset.UtcNow));

        LogHandled(outcome.SucceededCount, outcome.FailedCount);
        return outcome;
    }

    [LoggerMessage(LogLevel.Trace, "Handling bulk action '{Action}' for {Count} resource(s).")]
    private partial void LogHandling(string action, int count);

    [LoggerMessage(LogLevel.Trace, "Bulk action handled. Succeeded: {Succeeded}, Failed: {Failed}")]
    private partial void LogHandled(int succeeded, int failed);
}
