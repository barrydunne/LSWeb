using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.StepFunctions;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.StepFunctions;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.StartExecution;

internal sealed partial class StartExecutionCommandHandler
    : ICommandHandler<StartExecutionCommand, ExecutionStartResult>
{
    private const string OperationName = "step-functions-start-execution";

    private readonly IStepFunctionsClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public StartExecutionCommandHandler(
        IStepFunctionsClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<StartExecutionCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result<ExecutionStartResult>> Handle(
        StartExecutionCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.StateMachineArn);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Starting execution of {request.StateMachineArn}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.StartExecutionAsync(
            request.StateMachineArn, request.Name, request.Input, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to start execution of {request.StateMachineArn}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        await PublishOutcomeAsync(operationId, OperationState.Succeeded,
            $"Started execution {result.Value.ExecutionArn}.", cancellationToken);

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

    [LoggerMessage(LogLevel.Trace, "Starting Step Functions execution of {StateMachineArn}.")]
    private partial void LogHandling(string stateMachineArn);

    [LoggerMessage(LogLevel.Trace, "Step Functions start execution handled.")]
    private partial void LogHandled();
}
