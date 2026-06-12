using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Cognito;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.SetCognitoUserPassword;

internal sealed partial class SetCognitoUserPasswordCommandHandler : ICommandHandler<SetCognitoUserPasswordCommand>
{
    private const string OperationName = "cognito-set-user-password";

    private readonly ICognitoClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public SetCognitoUserPasswordCommandHandler(
        ICognitoClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<SetCognitoUserPasswordCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(SetCognitoUserPasswordCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.Username);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Setting password for {request.Username}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.SetUserPasswordAsync(
            request.UserPoolId, request.Username, request.Password, request.Permanent, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to set password for {request.Username}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        await PublishOutcomeAsync(operationId, OperationState.Succeeded,
            $"Set password for {request.Username}.", cancellationToken);

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

    [LoggerMessage(LogLevel.Trace, "Setting password for Cognito user {Username}.")]
    private partial void LogHandling(string username);

    [LoggerMessage(LogLevel.Trace, "Cognito set user password handled.")]
    private partial void LogHandled();
}
