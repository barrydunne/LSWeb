using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Lambda;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Lambda;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.InvokeLambdaFunction;

internal sealed partial class InvokeLambdaFunctionCommandHandler : ICommandHandler<InvokeLambdaFunctionCommand, LambdaInvocationResult>
{
    private const string OperationName = "lambda-invoke";

    private readonly ILambdaClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public InvokeLambdaFunctionCommandHandler(
        ILambdaClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<InvokeLambdaFunctionCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result<LambdaInvocationResult>> Handle(InvokeLambdaFunctionCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.FunctionName);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Invoking {request.FunctionName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var invocation = await _client.InvokeAsync(request.FunctionName, request.Payload, cancellationToken);
        if (!invocation.IsSuccess)
        {
            var failure = $"Failed to invoke {request.FunctionName}: {invocation.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return invocation.Error!.Value;
        }

        var result = invocation.Value;
        var state = result.HasFunctionError ? OperationState.Failed : OperationState.Succeeded;
        var message = result.HasFunctionError
            ? $"{request.FunctionName} returned a function error ({result.FunctionError})."
            : $"{request.FunctionName} invoked successfully in {result.DurationMs} ms.";
        await PublishOutcomeAsync(operationId, state, message, cancellationToken);

        LogHandled(result.StatusCode, result.HasFunctionError);
        return result;
    }

    private async Task PublishOutcomeAsync(string operationId, OperationState state, string message, CancellationToken cancellationToken)
    {
        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, state, message, DateTimeOffset.UtcNow),
            cancellationToken);
        _activityLog.Append(
            new ActivityEntry(operationId, OperationName, state, message, DateTimeOffset.UtcNow));
    }

    [LoggerMessage(LogLevel.Trace, "Invoking Lambda function {FunctionName}.")]
    private partial void LogHandling(string functionName);

    [LoggerMessage(LogLevel.Trace, "Lambda invoke handled. StatusCode: {StatusCode}, FunctionError: {HasFunctionError}")]
    private partial void LogHandled(int statusCode, bool hasFunctionError);
}
