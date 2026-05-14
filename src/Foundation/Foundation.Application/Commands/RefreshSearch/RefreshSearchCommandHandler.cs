using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.RefreshSearch;

internal sealed partial class RefreshSearchCommandHandler : ICommandHandler<RefreshSearchCommand>
{
    private const string OperationName = "search-refresh";

    private readonly ISearchRefreshTrigger _trigger;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public RefreshSearchCommandHandler(
        ISearchRefreshTrigger trigger,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<RefreshSearchCommandHandler> logger)
    {
        _trigger = trigger;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(RefreshSearchCommand request, CancellationToken cancellationToken)
    {
        LogHandling();
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress, "Refreshing the search index.", DateTimeOffset.UtcNow),
            cancellationToken);

        _trigger.RequestRefresh();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.Succeeded, "Search index refresh requested.", DateTimeOffset.UtcNow),
            cancellationToken);

        _activityLog.Append(
            new ActivityEntry(operationId, OperationName, OperationState.Succeeded, "Search index refresh requested.", DateTimeOffset.UtcNow));

        return Result.Success();
    }

    [LoggerMessage(LogLevel.Trace, "Handling search refresh command.")]
    private partial void LogHandling();
}
