using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Search;
using Foundation.Application.SecretsManager;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.SecretsManager;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.PutSecretValue;

internal sealed partial class PutSecretValueCommandHandler : ICommandHandler<PutSecretValueCommand>
{
    private const string OperationName = "secrets-manager-put-secret-value";

    private readonly ISecretsManagerClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public PutSecretValueCommandHandler(
        ISecretsManagerClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<PutSecretValueCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(PutSecretValueCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.SecretId);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Updating {request.SecretId}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var specification = new SecretValueSpecification(request.SecretId, request.SecretString);

        var result = await _client.PutSecretValueAsync(specification, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to update {request.SecretId}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Updated {request.SecretId}.";
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

    [LoggerMessage(LogLevel.Trace, "Updating Secrets Manager secret value for {SecretId}.")]
    private partial void LogHandling(string secretId);

    [LoggerMessage(LogLevel.Trace, "Secrets Manager secret value update handled.")]
    private partial void LogHandled();
}
