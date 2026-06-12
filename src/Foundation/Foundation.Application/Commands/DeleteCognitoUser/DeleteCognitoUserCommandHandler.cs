using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Cognito;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteCognitoUser;

internal sealed partial class DeleteCognitoUserCommandHandler : ICommandHandler<DeleteCognitoUserCommand>
{
    private const string OperationName = "cognito-delete-user";

    private readonly ICognitoClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public DeleteCognitoUserCommandHandler(
        ICognitoClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<DeleteCognitoUserCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteCognitoUserCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.Username);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Deleting {request.Username}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.DeleteUserAsync(request.UserPoolId, request.Username, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to delete {request.Username}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        await PublishOutcomeAsync(operationId, OperationState.Succeeded,
            $"Deleted {request.Username}.", cancellationToken);

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

    [LoggerMessage(LogLevel.Trace, "Deleting Cognito user {Username}.")]
    private partial void LogHandling(string username);

    [LoggerMessage(LogLevel.Trace, "Cognito delete user handled.")]
    private partial void LogHandled();
}
