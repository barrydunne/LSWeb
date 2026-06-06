using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.ApiGateway;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteRestMethod;

internal sealed partial class DeleteRestMethodCommandHandler
    : ICommandHandler<DeleteRestMethodCommand>
{
    private const string OperationName = "apigateway-delete-method";

    private readonly IApiGatewayClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public DeleteRestMethodCommandHandler(
        IApiGatewayClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<DeleteRestMethodCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(
        DeleteRestMethodCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.HttpMethod, request.ResourceId);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Deleting method {request.HttpMethod}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.DeleteMethodAsync(
            request.RestApiId, request.ResourceId, request.HttpMethod, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to delete method {request.HttpMethod}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        await PublishOutcomeAsync(operationId, OperationState.Succeeded,
            $"Deleted method {request.HttpMethod}.", cancellationToken);
        _searchRefresh.RequestRefresh();

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

    [LoggerMessage(LogLevel.Trace, "Deleting API Gateway REST API method {HttpMethod} on {ResourceId}.")]
    private partial void LogHandling(string httpMethod, string resourceId);

    [LoggerMessage(LogLevel.Trace, "API Gateway delete REST API method handled.")]
    private partial void LogHandled();
}
