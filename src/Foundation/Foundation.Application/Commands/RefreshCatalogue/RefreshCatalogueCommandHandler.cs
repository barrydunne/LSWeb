using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.RefreshCatalogue;

internal sealed partial class RefreshCatalogueCommandHandler : ICommandHandler<RefreshCatalogueCommand>
{
    private const string OperationName = "catalogue-refresh";

    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public RefreshCatalogueCommandHandler(INotificationPublisher publisher, IActivityLog activityLog, ILogger<RefreshCatalogueCommandHandler> logger)
    {
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(RefreshCatalogueCommand request, CancellationToken cancellationToken)
    {
        LogHandling();
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress, "Refreshing the service catalogue.", DateTimeOffset.UtcNow),
            cancellationToken);

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.Succeeded, "Service catalogue refreshed.", DateTimeOffset.UtcNow),
            cancellationToken);

        _activityLog.Append(
            new ActivityEntry(operationId, OperationName, OperationState.Succeeded, "Service catalogue refreshed.", DateTimeOffset.UtcNow));

        return Result.Success();
    }

    [LoggerMessage(LogLevel.Trace, "Handling catalogue refresh command.")]
    private partial void LogHandling();
}
