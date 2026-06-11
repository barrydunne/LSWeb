using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.CloudFormation;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateStack;

internal sealed partial class CreateStackCommandHandler : ICommandHandler<CreateStackCommand, string>
{
    private const string OperationName = "cloudformation-create-stack";

    private readonly ICloudFormationClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public CreateStackCommandHandler(
        ICloudFormationClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<CreateStackCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(CreateStackCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.StackName);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Creating {request.StackName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.CreateStackAsync(
            request.StackName, request.TemplateBody, request.TemplateUrl, request.Parameters, request.Capabilities, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to create {request.StackName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        await PublishOutcomeAsync(operationId, OperationState.Succeeded,
            $"Created {request.StackName}.", cancellationToken);
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

    [LoggerMessage(LogLevel.Trace, "Creating CloudFormation stack {StackName}.")]
    private partial void LogHandling(string stackName);

    [LoggerMessage(LogLevel.Trace, "CloudFormation create stack handled.")]
    private partial void LogHandled();
}
