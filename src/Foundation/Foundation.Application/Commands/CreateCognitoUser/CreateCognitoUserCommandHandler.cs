using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Cognito;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Cognito;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateCognitoUser;

internal sealed partial class CreateCognitoUserCommandHandler : ICommandHandler<CreateCognitoUserCommand, CognitoUserDetail>
{
    private const string OperationName = "cognito-create-user";

    private readonly ICognitoClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public CreateCognitoUserCommandHandler(
        ICognitoClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<CreateCognitoUserCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result<CognitoUserDetail>> Handle(CreateCognitoUserCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.Username);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Creating {request.Username}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var specification = new CognitoUserSpecification(
            request.UserPoolId,
            request.Username,
            request.Attributes,
            request.TemporaryPassword);
        var result = await _client.CreateUserAsync(specification, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to create {request.Username}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        await PublishOutcomeAsync(operationId, OperationState.Succeeded,
            $"Created {request.Username}.", cancellationToken);

        LogHandled();
        return result.Value;
    }

    private async Task PublishOutcomeAsync(string operationId, OperationState state, string message, CancellationToken cancellationToken)
    {
        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, state, message, DateTimeOffset.UtcNow),
            cancellationToken);
        _activityLog.Append(
            new ActivityEntry(operationId, OperationName, state, message, DateTimeOffset.UtcNow));
    }

    [LoggerMessage(LogLevel.Trace, "Creating Cognito user {Username}.")]
    private partial void LogHandling(string username);

    [LoggerMessage(LogLevel.Trace, "Cognito create user handled.")]
    private partial void LogHandled();
}
