using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Lambda;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Configuration;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UpdateLambdaEnvironment;

internal sealed partial class UpdateLambdaEnvironmentCommandHandler : ICommandHandler<UpdateLambdaEnvironmentCommand>
{
    private const string OperationName = "lambda-environment-update";

    private readonly ILambdaClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public UpdateLambdaEnvironmentCommandHandler(
        ILambdaClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<UpdateLambdaEnvironmentCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateLambdaEnvironmentCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.FunctionName, request.Variables.Count);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Updating environment variables for {request.FunctionName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var current = await _client.GetEnvironmentAsync(request.FunctionName, cancellationToken);
        if (!current.IsSuccess)
            return await FailAsync(operationId, request.FunctionName, current.Error!.Value, cancellationToken);

        var merged = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (name, value) in request.Variables)
        {
            merged[name] = value == ConfigValue.Mask && current.Value.TryGetValue(name, out var existing)
                ? existing
                : value;
        }

        var update = await _client.UpdateEnvironmentAsync(request.FunctionName, merged, cancellationToken);
        if (!update.IsSuccess)
            return await FailAsync(operationId, request.FunctionName, update.Error!.Value, cancellationToken);

        var message = $"Environment variables updated for {request.FunctionName}.";
        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.Succeeded, message, DateTimeOffset.UtcNow),
            cancellationToken);
        _activityLog.Append(
            new ActivityEntry(operationId, OperationName, OperationState.Succeeded, message, DateTimeOffset.UtcNow));

        LogHandled();
        return Result.Success();
    }

    private async Task<Result> FailAsync(string operationId, string functionName, Error error, CancellationToken cancellationToken)
    {
        var message = $"Failed to update environment variables for {functionName}: {error.Message}";
        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.Failed, message, DateTimeOffset.UtcNow),
            cancellationToken);
        _activityLog.Append(
            new ActivityEntry(operationId, OperationName, OperationState.Failed, message, DateTimeOffset.UtcNow));

        return error;
    }

    [LoggerMessage(LogLevel.Trace, "Updating Lambda environment for {FunctionName} with {Count} variable(s).")]
    private partial void LogHandling(string functionName, int count);

    [LoggerMessage(LogLevel.Trace, "Lambda environment update handled.")]
    private partial void LogHandled();
}
