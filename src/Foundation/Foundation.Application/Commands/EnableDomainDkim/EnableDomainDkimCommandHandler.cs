using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Ses;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.EnableDomainDkim;

internal sealed partial class EnableDomainDkimCommandHandler : ICommandHandler<EnableDomainDkimCommand>
{
    private const string OperationName = "ses-enable-domain-dkim";

    private readonly ISesClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public EnableDomainDkimCommandHandler(
        ISesClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<EnableDomainDkimCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(EnableDomainDkimCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.Domain);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Enabling DKIM for {request.Domain}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.EnableDomainDkimAsync(request.Domain, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to enable DKIM for {request.Domain}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Enabled DKIM for {request.Domain}.";
        await PublishOutcomeAsync(operationId, OperationState.Succeeded, message, cancellationToken);

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

    [LoggerMessage(LogLevel.Trace, "Enabling DKIM for SES domain {Domain}.")]
    private partial void LogHandling(string domain);

    [LoggerMessage(LogLevel.Trace, "SES domain DKIM enable handled.")]
    private partial void LogHandled();
}
