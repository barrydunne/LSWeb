using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.StepFunctions;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UpdateStateMachineDefinition;

internal sealed partial class UpdateStateMachineDefinitionCommandHandler
    : ICommandHandler<UpdateStateMachineDefinitionCommand>
{
    private const string OperationName = "step-functions-update-definition";

    private readonly IStepFunctionsClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public UpdateStateMachineDefinitionCommandHandler(
        IStepFunctionsClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<UpdateStateMachineDefinitionCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateStateMachineDefinitionCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.StateMachineArn);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Updating the definition of {request.StateMachineArn}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.UpdateStateMachineDefinitionAsync(
            request.StateMachineArn, request.Definition, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to update the definition of {request.StateMachineArn}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Updated the definition of {request.StateMachineArn}.";
        await PublishOutcomeAsync(operationId, OperationState.Succeeded, message, cancellationToken);

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

    [LoggerMessage(LogLevel.Trace, "Updating the definition of Step Functions state machine {StateMachineArn}.")]
    private partial void LogHandling(string stateMachineArn);

    [LoggerMessage(LogLevel.Trace, "Step Functions update definition handled.")]
    private partial void LogHandled();
}
