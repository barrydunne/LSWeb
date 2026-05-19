using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.Lambda;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Lambda;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.UpdateLambdaFunction;

internal sealed partial class UpdateLambdaFunctionCommandHandler : ICommandHandler<UpdateLambdaFunctionCommand>
{
    private const string OperationName = "lambda-update";

    private readonly ILambdaClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public UpdateLambdaFunctionCommandHandler(
        ILambdaClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<UpdateLambdaFunctionCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateLambdaFunctionCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.FunctionName);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Updating {request.FunctionName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var spec = new LambdaConfigurationUpdateSpec(
            request.FunctionName,
            request.Runtime,
            request.Handler,
            request.Role,
            request.Description,
            request.MemorySize,
            request.Timeout);
        var configuration = await _client.UpdateConfigurationAsync(spec, cancellationToken);
        if (!configuration.IsSuccess)
            return await FailAsync(operationId, request.FunctionName, configuration.Error!.Value, cancellationToken);

        if (!string.IsNullOrEmpty(request.ZipFileBase64))
        {
            var code = await _client.UpdateCodeAsync(request.FunctionName, request.ZipFileBase64, cancellationToken);
            if (!code.IsSuccess)
                return await FailAsync(operationId, request.FunctionName, code.Error!.Value, cancellationToken);
        }

        var message = $"Updated {request.FunctionName}.";
        await PublishOutcomeAsync(operationId, OperationState.Succeeded, message, cancellationToken);
        _searchRefresh.RequestRefresh();

        LogHandled();
        return Result.Success();
    }

    private async Task<Result> FailAsync(string operationId, string functionName, Error error, CancellationToken cancellationToken)
    {
        var message = $"Failed to update {functionName}: {error.Message}";
        await PublishOutcomeAsync(operationId, OperationState.Failed, message, cancellationToken);
        return error;
    }

    private async Task PublishOutcomeAsync(string operationId, OperationState state, string message, CancellationToken cancellationToken)
    {
        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, state, message, DateTimeOffset.UtcNow),
            cancellationToken);
        _activityLog.Append(
            new ActivityEntry(operationId, OperationName, state, message, DateTimeOffset.UtcNow));
    }

    [LoggerMessage(LogLevel.Trace, "Updating Lambda function {FunctionName}.")]
    private partial void LogHandling(string functionName);

    [LoggerMessage(LogLevel.Trace, "Lambda function update handled.")]
    private partial void LogHandled();
}
