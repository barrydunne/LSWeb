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

namespace Foundation.Application.Commands.UpdateUserPoolClient;

internal sealed partial class UpdateUserPoolClientCommandHandler : ICommandHandler<UpdateUserPoolClientCommand>
{
    private const string OperationName = "cognito-update-client";

    private readonly ICognitoClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public UpdateUserPoolClientCommandHandler(
        ICognitoClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<UpdateUserPoolClientCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateUserPoolClientCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.ClientId);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Updating {request.ClientName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var specification = new UserPoolClientSpecification(
            request.UserPoolId,
            request.ClientName,
            GenerateSecret: false,
            request.ExplicitAuthFlows,
            request.AllowedOAuthFlows,
            request.AllowedOAuthScopes,
            request.CallbackURLs,
            request.AllowedOAuthFlowsUserPoolClient,
            request.ClientId);
        var result = await _client.UpdateUserPoolClientAsync(specification, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to update {request.ClientName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        await PublishOutcomeAsync(operationId, OperationState.Succeeded,
            $"Updated {request.ClientName}.", cancellationToken);
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

    [LoggerMessage(LogLevel.Trace, "Updating Cognito app client {ClientId}.")]
    private partial void LogHandling(string clientId);

    [LoggerMessage(LogLevel.Trace, "Cognito update app client handled.")]
    private partial void LogHandled();
}
