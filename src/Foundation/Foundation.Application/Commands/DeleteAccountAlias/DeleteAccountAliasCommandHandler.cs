using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Iam;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteAccountAlias;

internal sealed partial class DeleteAccountAliasCommandHandler : ICommandHandler<DeleteAccountAliasCommand>
{
    private const string OperationName = "iam-delete-account-alias";

    private readonly IIamClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public DeleteAccountAliasCommandHandler(
        IIamClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<DeleteAccountAliasCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteAccountAliasCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.AccountAlias);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Deleting account alias {request.AccountAlias}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.DeleteAccountAliasAsync(request.AccountAlias, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to delete account alias {request.AccountAlias}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        await PublishOutcomeAsync(operationId, OperationState.Succeeded,
            $"Deleted account alias {request.AccountAlias}.", cancellationToken);

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

    [LoggerMessage(LogLevel.Trace, "Deleting IAM account alias {AccountAlias}.")]
    private partial void LogHandling(string accountAlias);

    [LoggerMessage(LogLevel.Trace, "IAM account alias delete handled.")]
    private partial void LogHandled();
}
