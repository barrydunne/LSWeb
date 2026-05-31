using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Iam;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeletePolicyVersion;

internal sealed partial class DeletePolicyVersionCommandHandler : ICommandHandler<DeletePolicyVersionCommand>
{
    private const string OperationName = "iam-delete-policy-version";

    private readonly IIamClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public DeletePolicyVersionCommandHandler(
        IIamClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<DeletePolicyVersionCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(DeletePolicyVersionCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.PolicyArn, request.VersionId);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Deleting version {request.VersionId} of {request.PolicyArn}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var policy = await _client.GetPolicyAsync(request.PolicyArn, cancellationToken);
        if (!policy.IsSuccess)
        {
            var lookupFailure = $"Failed to delete version {request.VersionId} of {request.PolicyArn}: {policy.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, lookupFailure, cancellationToken);
            return policy.Error!.Value;
        }

        if (string.Equals(policy.Value.DefaultVersionId, request.VersionId, StringComparison.Ordinal))
        {
            var guard = new Error("Cannot delete the default version of a policy. Set another version as default first.");
            await PublishOutcomeAsync(operationId, OperationState.Failed, guard.Message, cancellationToken);
            return guard;
        }

        var result = await _client.DeletePolicyVersionAsync(request.PolicyArn, request.VersionId, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to delete version {request.VersionId} of {request.PolicyArn}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Deleted version {request.VersionId} of {request.PolicyArn}.";
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

    [LoggerMessage(LogLevel.Trace, "Deleting version {VersionId} of IAM policy {PolicyArn}.")]
    private partial void LogHandling(string policyArn, string versionId);

    [LoggerMessage(LogLevel.Trace, "IAM policy version delete handled.")]
    private partial void LogHandled();
}
