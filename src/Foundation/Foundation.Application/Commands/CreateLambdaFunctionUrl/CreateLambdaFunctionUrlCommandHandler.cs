using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Lambda;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateLambdaFunctionUrl;

internal sealed partial class CreateLambdaFunctionUrlCommandHandler : ICommandHandler<CreateLambdaFunctionUrlCommand>
{
    private const string OperationName = "lambda-function-url-create";

    private readonly ILambdaClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public CreateLambdaFunctionUrlCommandHandler(
        ILambdaClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<CreateLambdaFunctionUrlCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result> Handle(CreateLambdaFunctionUrlCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.FunctionName, request.AuthType);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Creating function URL for {request.FunctionName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.CreateFunctionUrlAsync(request.FunctionName, request.AuthType, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to create function URL for {request.FunctionName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        var message = $"Created function URL for {request.FunctionName}.";
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

    [LoggerMessage(LogLevel.Trace, "Creating Lambda function URL for {FunctionName} with auth {AuthType}.")]
    private partial void LogHandling(string functionName, string authType);

    [LoggerMessage(LogLevel.Trace, "Lambda function URL creation handled.")]
    private partial void LogHandled();
}
