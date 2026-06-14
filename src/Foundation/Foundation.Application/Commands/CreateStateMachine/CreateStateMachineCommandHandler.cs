using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Search;
using Foundation.Application.StepFunctions;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.StepFunctions;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateStateMachine;

internal sealed partial class CreateStateMachineCommandHandler
    : ICommandHandler<CreateStateMachineCommand, StateMachineCreateResult>
{
    private const string OperationName = "step-functions-create-state-machine";

    private readonly IStepFunctionsClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public CreateStateMachineCommandHandler(
        IStepFunctionsClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<CreateStateMachineCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result<StateMachineCreateResult>> Handle(
        CreateStateMachineCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.Name);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Creating state machine {request.Name}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.CreateStateMachineAsync(
            request.Name, request.Definition, request.RoleArn, request.Type, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to create state machine {request.Name}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        await PublishOutcomeAsync(operationId, OperationState.Succeeded,
            $"Created state machine {result.Value.StateMachineArn}.", cancellationToken);
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

    [LoggerMessage(LogLevel.Trace, "Creating Step Functions state machine {Name}.")]
    private partial void LogHandling(string name);

    [LoggerMessage(LogLevel.Trace, "Step Functions create state machine handled.")]
    private partial void LogHandled();
}
