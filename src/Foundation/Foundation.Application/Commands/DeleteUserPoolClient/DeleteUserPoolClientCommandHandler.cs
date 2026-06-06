using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Cognito;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteUserPoolClient;

internal sealed partial class DeleteUserPoolClientCommandHandler : ICommandHandler<DeleteUserPoolClientCommand>
{
    private const string OperationName = "cognito-delete-client";

    private readonly ICognitoClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public DeleteUserPoolClientCommandHandler(
        ICognitoClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<DeleteUserPoolClientCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteUserPoolClientCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.ClientId);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Deleting {request.ClientId}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.DeleteUserPoolClientAsync(request.UserPoolId, request.ClientId, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to delete {request.ClientId}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Deleted {request.ClientId}.";
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

    [LoggerMessage(LogLevel.Trace, "Deleting Cognito app client {ClientId}.")]
    private partial void LogHandling(string clientId);

    [LoggerMessage(LogLevel.Trace, "Cognito delete app client handled.")]
    private partial void LogHandled();
}
