using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.CloudFormation;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateChangeSet;

internal sealed partial class CreateChangeSetCommandHandler : ICommandHandler<CreateChangeSetCommand, string>
{
    private const string OperationName = "cloudformation-create-change-set";

    private readonly ICloudFormationClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public CreateChangeSetCommandHandler(
        ICloudFormationClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<CreateChangeSetCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(CreateChangeSetCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.ChangeSetName, request.StackName);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Creating change set {request.ChangeSetName} for {request.StackName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.CreateChangeSetAsync(
            request.StackName, request.ChangeSetName, request.ChangeSetType,
            request.TemplateBody, request.Parameters, request.Capabilities, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to create change set {request.ChangeSetName} for {request.StackName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        await PublishOutcomeAsync(operationId, OperationState.Succeeded,
            $"Created change set {request.ChangeSetName} for {request.StackName}.", cancellationToken);
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

    [LoggerMessage(LogLevel.Trace, "Creating CloudFormation change set {ChangeSetName} for {StackName}.")]
    private partial void LogHandling(string changeSetName, string stackName);

    [LoggerMessage(LogLevel.Trace, "CloudFormation create change set handled.")]
    private partial void LogHandled();
}
