using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.CloudFormation;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.ExecuteChangeSet;

internal sealed partial class ExecuteChangeSetCommandHandler : ICommandHandler<ExecuteChangeSetCommand>
{
    private const string OperationName = "cloudformation-execute-change-set";

    private readonly ICloudFormationClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public ExecuteChangeSetCommandHandler(
        ICloudFormationClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<ExecuteChangeSetCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(ExecuteChangeSetCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.ChangeSetName, request.StackName);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Executing change set {request.ChangeSetName} for {request.StackName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.ExecuteChangeSetAsync(
            request.StackName, request.ChangeSetName, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to execute change set {request.ChangeSetName} for {request.StackName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        await PublishOutcomeAsync(operationId, OperationState.Succeeded,
            $"Executed change set {request.ChangeSetName} for {request.StackName}.", cancellationToken);
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

    [LoggerMessage(LogLevel.Trace, "Executing CloudFormation change set {ChangeSetName} for {StackName}.")]
    private partial void LogHandling(string changeSetName, string stackName);

    [LoggerMessage(LogLevel.Trace, "CloudFormation execute change set handled.")]
    private partial void LogHandled();
}
