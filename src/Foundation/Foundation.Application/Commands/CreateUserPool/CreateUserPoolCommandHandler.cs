using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Cognito;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Cognito;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateUserPool;

internal sealed partial class CreateUserPoolCommandHandler : ICommandHandler<CreateUserPoolCommand, string>
{
    private const string OperationName = "cognito-create-pool";

    private readonly ICognitoClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public CreateUserPoolCommandHandler(
        ICognitoClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<CreateUserPoolCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(CreateUserPoolCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.Name);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Creating {request.Name}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var specification = new UserPoolSpecification(
            request.Name, request.MfaConfiguration, request.UsernameAttributes, request.AutoVerifiedAttributes);
        var result = await _client.CreateUserPoolAsync(specification, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to create {request.Name}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        await PublishOutcomeAsync(operationId, OperationState.Succeeded,
            $"Created {request.Name}.", cancellationToken);
        _searchRefresh.RequestRefresh();

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

    [LoggerMessage(LogLevel.Trace, "Creating Cognito user pool {Name}.")]
    private partial void LogHandling(string name);

    [LoggerMessage(LogLevel.Trace, "Cognito create user pool handled.")]
    private partial void LogHandled();
}
