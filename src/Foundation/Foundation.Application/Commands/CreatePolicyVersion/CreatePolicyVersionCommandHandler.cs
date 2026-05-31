using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Iam;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreatePolicyVersion;

internal sealed partial class CreatePolicyVersionCommandHandler : ICommandHandler<CreatePolicyVersionCommand>
{
    private const string OperationName = "iam-create-policy-version";

    private readonly IIamClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public CreatePolicyVersionCommandHandler(
        IIamClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<CreatePolicyVersionCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(CreatePolicyVersionCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.PolicyArn);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Creating a new version of {request.PolicyArn}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.CreatePolicyVersionAsync(
            request.PolicyArn, request.PolicyDocument, request.SetAsDefault, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to create a new version of {request.PolicyArn}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Created a new version of {request.PolicyArn}.";
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

    [LoggerMessage(LogLevel.Trace, "Creating a new version of IAM policy {PolicyArn}.")]
    private partial void LogHandling(string policyArn);

    [LoggerMessage(LogLevel.Trace, "IAM policy version create handled.")]
    private partial void LogHandled();
}
