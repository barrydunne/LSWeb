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

namespace Foundation.Application.Commands.RegenerateUserPoolClientSecret;

internal sealed partial class RegenerateUserPoolClientSecretCommandHandler : ICommandHandler<RegenerateUserPoolClientSecretCommand, UserPoolClientDetail>
{
    private const string OperationName = "cognito-regenerate-client-secret";

    private readonly ICognitoClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public RegenerateUserPoolClientSecretCommandHandler(
        ICognitoClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<RegenerateUserPoolClientSecretCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result<UserPoolClientDetail>> Handle(RegenerateUserPoolClientSecretCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.ClientId);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Regenerating secret for {request.ClientId}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.RegenerateUserPoolClientSecretAsync(request.UserPoolId, request.ClientId, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to regenerate secret for {request.ClientId}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        await PublishOutcomeAsync(operationId, OperationState.Succeeded,
            $"Regenerated secret for {request.ClientId} as {result.Value.ClientId}.", cancellationToken);
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

    [LoggerMessage(LogLevel.Trace, "Regenerating Cognito app client secret for {ClientId}.")]
    private partial void LogHandling(string clientId);

    [LoggerMessage(LogLevel.Trace, "Cognito regenerate app client secret handled.")]
    private partial void LogHandled();
}
