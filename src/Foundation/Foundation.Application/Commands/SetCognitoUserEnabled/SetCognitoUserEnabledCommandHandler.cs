using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Cognito;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.SetCognitoUserEnabled;

internal sealed partial class SetCognitoUserEnabledCommandHandler : ICommandHandler<SetCognitoUserEnabledCommand>
{
    private const string OperationName = "cognito-set-user-enabled";

    private readonly ICognitoClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public SetCognitoUserEnabledCommandHandler(
        ICognitoClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<SetCognitoUserEnabledCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(SetCognitoUserEnabledCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.Username, request.Enabled);
        var operationId = Guid.NewGuid().ToString();
        var verb = request.Enabled ? "Enabling" : "Disabling";

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"{verb} {request.Username}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.SetUserEnabledAsync(
            request.UserPoolId, request.Username, request.Enabled, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to update {request.Username}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var outcome = request.Enabled ? "Enabled" : "Disabled";
        await PublishOutcomeAsync(operationId, OperationState.Succeeded,
            $"{outcome} {request.Username}.", cancellationToken);

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

    [LoggerMessage(LogLevel.Trace, "Setting enabled state {Enabled} for Cognito user {Username}.")]
    private partial void LogHandling(string username, bool enabled);

    [LoggerMessage(LogLevel.Trace, "Cognito set user enabled handled.")]
    private partial void LogHandled();
}
