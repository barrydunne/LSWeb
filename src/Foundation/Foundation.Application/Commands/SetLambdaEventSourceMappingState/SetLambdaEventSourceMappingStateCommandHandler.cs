using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Lambda;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.SetLambdaEventSourceMappingState;

internal sealed partial class SetLambdaEventSourceMappingStateCommandHandler : ICommandHandler<SetLambdaEventSourceMappingStateCommand>
{
    private const string OperationName = "lambda-event-source-mapping-state";

    private readonly ILambdaClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public SetLambdaEventSourceMappingStateCommandHandler(
        ILambdaClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<SetLambdaEventSourceMappingStateCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(SetLambdaEventSourceMappingStateCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.FunctionName, request.Uuid, request.Enabled);
        var operationId = Guid.NewGuid().ToString();
        var verb = request.Enabled ? "Enabling" : "Disabling";

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"{verb} event source mapping for {request.FunctionName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.SetEventSourceMappingStateAsync(request.Uuid, request.Enabled, cancellationToken);
        if (!result.IsSuccess)
        {
            var pastVerb = request.Enabled ? "enable" : "disable";
            var failure = $"Failed to {pastVerb} event source mapping for {request.FunctionName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var pastTense = request.Enabled ? "Enabled" : "Disabled";
        var message = $"{pastTense} event source mapping for {request.FunctionName}.";
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

    [LoggerMessage(LogLevel.Trace, "Setting Lambda event source mapping {Uuid} for {FunctionName} enabled: {Enabled}.")]
    private partial void LogHandling(string functionName, string uuid, bool enabled);

    [LoggerMessage(LogLevel.Trace, "Lambda event source mapping state change handled.")]
    private partial void LogHandled();
}
