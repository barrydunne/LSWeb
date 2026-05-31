using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Iam;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteAccountPasswordPolicy;

internal sealed partial class DeleteAccountPasswordPolicyCommandHandler : ICommandHandler<DeleteAccountPasswordPolicyCommand>
{
    private const string OperationName = "iam-delete-account-password-policy";

    private readonly IIamClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public DeleteAccountPasswordPolicyCommandHandler(
        IIamClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<DeleteAccountPasswordPolicyCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteAccountPasswordPolicyCommand request, CancellationToken cancellationToken)
    {
        LogHandling();
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                "Deleting account password policy.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.DeleteAccountPasswordPolicyAsync(cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to delete account password policy: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        await PublishOutcomeAsync(operationId, OperationState.Succeeded, "Deleted account password policy.", cancellationToken);

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

    [LoggerMessage(LogLevel.Trace, "Deleting IAM account password policy.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "IAM account password policy delete handled.")]
    private partial void LogHandled();
}
