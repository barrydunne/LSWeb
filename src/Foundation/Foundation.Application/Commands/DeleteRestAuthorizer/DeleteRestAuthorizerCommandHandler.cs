using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.ApiGateway;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.DeleteRestAuthorizer;

internal sealed partial class DeleteRestAuthorizerCommandHandler
    : ICommandHandler<DeleteRestAuthorizerCommand>
{
    private const string OperationName = "apigateway-delete-authorizer";

    private readonly IApiGatewayClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public DeleteRestAuthorizerCommandHandler(
        IApiGatewayClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<DeleteRestAuthorizerCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(
        DeleteRestAuthorizerCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.AuthorizerId, request.RestApiId);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Deleting authorizer {request.AuthorizerId}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.DeleteAuthorizerAsync(
            request.RestApiId, request.AuthorizerId, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to delete authorizer {request.AuthorizerId}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        await PublishOutcomeAsync(operationId, OperationState.Succeeded,
            $"Deleted authorizer {request.AuthorizerId}.", cancellationToken);
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

    [LoggerMessage(LogLevel.Trace, "Deleting API Gateway REST API authorizer {AuthorizerId} on {RestApiId}.")]
    private partial void LogHandling(string authorizerId, string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway delete REST API authorizer handled.")]
    private partial void LogHandled();
}
