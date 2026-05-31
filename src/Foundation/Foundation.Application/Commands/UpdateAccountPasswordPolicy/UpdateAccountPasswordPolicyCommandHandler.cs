using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Iam;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Iam;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UpdateAccountPasswordPolicy;

internal sealed partial class UpdateAccountPasswordPolicyCommandHandler : ICommandHandler<UpdateAccountPasswordPolicyCommand>
{
    private const string OperationName = "iam-update-account-password-policy";

    private readonly IIamClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public UpdateAccountPasswordPolicyCommandHandler(
        IIamClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<UpdateAccountPasswordPolicyCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateAccountPasswordPolicyCommand request, CancellationToken cancellationToken)
    {
        LogHandling();
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                "Updating account password policy.", DateTimeOffset.UtcNow),
            cancellationToken);

        var policy = new IamPasswordPolicy(
            request.MinimumPasswordLength,
            request.RequireSymbols,
            request.RequireNumbers,
            request.RequireUppercaseCharacters,
            request.RequireLowercaseCharacters,
            request.AllowUsersToChangePassword,
            request.MaxPasswordAge.HasValue,
            request.MaxPasswordAge,
            request.PasswordReusePrevention,
            request.HardExpiry);

        var result = await _client.UpdateAccountPasswordPolicyAsync(policy, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to update account password policy: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        await PublishOutcomeAsync(operationId, OperationState.Succeeded, "Updated account password policy.", cancellationToken);

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

    [LoggerMessage(LogLevel.Trace, "Updating IAM account password policy.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "IAM account password policy update handled.")]
    private partial void LogHandled();
}
