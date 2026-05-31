using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Iam;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.PutUserPermissionsBoundary;

internal sealed partial class PutUserPermissionsBoundaryCommandHandler : ICommandHandler<PutUserPermissionsBoundaryCommand>
{
    private const string OperationName = "iam-put-user-permissions-boundary";

    private readonly IIamClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public PutUserPermissionsBoundaryCommandHandler(
        IIamClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<PutUserPermissionsBoundaryCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(PutUserPermissionsBoundaryCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.UserName, request.PermissionsBoundaryArn);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Setting permissions boundary {request.PermissionsBoundaryArn} on {request.UserName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.PutUserPermissionsBoundaryAsync(request.UserName, request.PermissionsBoundaryArn, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to set permissions boundary on {request.UserName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Set permissions boundary {request.PermissionsBoundaryArn} on {request.UserName}.";
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

    [LoggerMessage(LogLevel.Trace, "Setting permissions boundary {PermissionsBoundaryArn} on IAM user {UserName}.")]
    private partial void LogHandling(string userName, string permissionsBoundaryArn);

    [LoggerMessage(LogLevel.Trace, "IAM put user permissions boundary handled.")]
    private partial void LogHandled();
}
