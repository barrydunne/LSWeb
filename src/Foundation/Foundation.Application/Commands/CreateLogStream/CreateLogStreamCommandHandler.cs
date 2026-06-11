using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.CloudWatchLogs;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateLogStream;

internal sealed partial class CreateLogStreamCommandHandler : ICommandHandler<CreateLogStreamCommand>
{
    private const string OperationName = "cloudwatch-logs-create-stream";

    private readonly ICloudWatchLogsClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public CreateLogStreamCommandHandler(
        ICloudWatchLogsClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<CreateLogStreamCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(CreateLogStreamCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.LogGroupName, request.LogStreamName);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Creating {request.LogStreamName} in {request.LogGroupName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.CreateLogStreamAsync(
            request.LogGroupName, request.LogStreamName, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to create {request.LogStreamName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Created {request.LogStreamName} in {request.LogGroupName}.";
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

    [LoggerMessage(LogLevel.Trace, "Creating CloudWatch log stream {LogStreamName} in {LogGroupName}.")]
    private partial void LogHandling(string logGroupName, string logStreamName);

    [LoggerMessage(LogLevel.Trace, "CloudWatch log stream create handled.")]
    private partial void LogHandled();
}
