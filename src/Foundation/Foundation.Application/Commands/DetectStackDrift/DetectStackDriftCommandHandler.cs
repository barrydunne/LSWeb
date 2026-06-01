using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.CloudFormation;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DetectStackDrift;

internal sealed partial class DetectStackDriftCommandHandler : ICommandHandler<DetectStackDriftCommand, string>
{
    private const string OperationName = "cloudformation-detect-stack-drift";

    private readonly ICloudFormationClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public DetectStackDriftCommandHandler(
        ICloudFormationClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<DetectStackDriftCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(DetectStackDriftCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.StackName);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Detecting drift on {request.StackName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.DetectStackDriftAsync(request.StackName, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to detect drift on {request.StackName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        await PublishOutcomeAsync(operationId, OperationState.Succeeded,
            $"Started drift detection on {request.StackName}.", cancellationToken);

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

    [LoggerMessage(LogLevel.Trace, "Detecting CloudFormation drift on {StackName}.")]
    private partial void LogHandling(string stackName);

    [LoggerMessage(LogLevel.Trace, "CloudFormation detect stack drift handled.")]
    private partial void LogHandled();
}
