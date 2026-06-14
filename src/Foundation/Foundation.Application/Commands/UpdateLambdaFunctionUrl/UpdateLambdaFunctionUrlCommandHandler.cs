using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Lambda;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UpdateLambdaFunctionUrl;

internal sealed partial class UpdateLambdaFunctionUrlCommandHandler : ICommandHandler<UpdateLambdaFunctionUrlCommand>
{
    private const string OperationName = "lambda-function-url-update";

    private readonly ILambdaClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public UpdateLambdaFunctionUrlCommandHandler(
        ILambdaClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<UpdateLambdaFunctionUrlCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateLambdaFunctionUrlCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.FunctionName, request.AuthType);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Updating function URL for {request.FunctionName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.UpdateFunctionUrlAsync(request.FunctionName, request.AuthType, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to update function URL for {request.FunctionName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Updated function URL for {request.FunctionName}.";
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

    [LoggerMessage(LogLevel.Trace, "Updating Lambda function URL for {FunctionName} with auth {AuthType}.")]
    private partial void LogHandling(string functionName, string authType);

    [LoggerMessage(LogLevel.Trace, "Lambda function URL update handled.")]
    private partial void LogHandled();
}
