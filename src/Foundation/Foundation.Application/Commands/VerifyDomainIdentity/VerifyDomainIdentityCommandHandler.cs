using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Search;
using Foundation.Application.Ses;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.VerifyDomainIdentity;

internal sealed partial class VerifyDomainIdentityCommandHandler : ICommandHandler<VerifyDomainIdentityCommand>
{
    private const string OperationName = "ses-verify-domain-identity";

    private readonly ISesClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public VerifyDomainIdentityCommandHandler(
        ISesClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<VerifyDomainIdentityCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(VerifyDomainIdentityCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.Domain);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Initiating verification of {request.Domain}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.VerifyDomainIdentityAsync(request.Domain, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to initiate verification of {request.Domain}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Initiated verification of {request.Domain}.";
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

    [LoggerMessage(LogLevel.Trace, "Initiating verification of SES domain identity {Domain}.")]
    private partial void LogHandling(string domain);

    [LoggerMessage(LogLevel.Trace, "SES domain identity verification handled.")]
    private partial void LogHandled();
}
