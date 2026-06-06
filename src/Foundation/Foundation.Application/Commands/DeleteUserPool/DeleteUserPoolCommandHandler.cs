using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Cognito;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteUserPool;

internal sealed partial class DeleteUserPoolCommandHandler : ICommandHandler<DeleteUserPoolCommand>
{
    private const string OperationName = "cognito-delete-pool";

    private readonly ICognitoClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public DeleteUserPoolCommandHandler(
        ICognitoClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<DeleteUserPoolCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteUserPoolCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.Id);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Deleting {request.Id}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.DeleteUserPoolAsync(request.Id, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to delete {request.Id}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Deleted {request.Id}.";
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

    [LoggerMessage(LogLevel.Trace, "Deleting Cognito user pool {Id}.")]
    private partial void LogHandling(string id);

    [LoggerMessage(LogLevel.Trace, "Cognito delete user pool handled.")]
    private partial void LogHandled();
}
