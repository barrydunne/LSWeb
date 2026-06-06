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

namespace Foundation.Application.Commands.CreateUserPoolClient;

internal sealed partial class CreateUserPoolClientCommandHandler : ICommandHandler<CreateUserPoolClientCommand, UserPoolClientDetail>
{
    private const string OperationName = "cognito-create-client";

    private readonly ICognitoClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public CreateUserPoolClientCommandHandler(
        ICognitoClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<CreateUserPoolClientCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result<UserPoolClientDetail>> Handle(CreateUserPoolClientCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.ClientName);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Creating {request.ClientName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var specification = new UserPoolClientSpecification(
            request.UserPoolId,
            request.ClientName,
            request.GenerateSecret,
            request.ExplicitAuthFlows,
            request.AllowedOAuthFlows,
            request.AllowedOAuthScopes,
            request.CallbackURLs,
            request.AllowedOAuthFlowsUserPoolClient);
        var result = await _client.CreateUserPoolClientAsync(specification, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to create {request.ClientName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        await PublishOutcomeAsync(operationId, OperationState.Succeeded,
            $"Created {request.ClientName}.", cancellationToken);
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

    [LoggerMessage(LogLevel.Trace, "Creating Cognito app client {ClientName}.")]
    private partial void LogHandling(string clientName);

    [LoggerMessage(LogLevel.Trace, "Cognito create app client handled.")]
    private partial void LogHandled();
}
