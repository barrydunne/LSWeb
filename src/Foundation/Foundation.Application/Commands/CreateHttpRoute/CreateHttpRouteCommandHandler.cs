using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.ApiGatewayV2;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.ApiGatewayV2;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.CreateHttpRoute;

internal sealed partial class CreateHttpRouteCommandHandler : ICommandHandler<CreateHttpRouteCommand, string>
{
    private const string OperationName = "apigatewayv2-create-route";

    private readonly IApiGatewayV2Client _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public CreateHttpRouteCommandHandler(
        IApiGatewayV2Client client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<CreateHttpRouteCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(CreateHttpRouteCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.RouteKey, request.ApiId);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Creating route {request.RouteKey}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var specification = new HttpRouteSpecification(
            request.ApiId,
            null,
            request.RouteKey,
            request.Target,
            request.AuthorizationType,
            request.AuthorizerId,
            request.AuthorizationScopes);
        var result = await _client.CreateRouteAsync(specification, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to create route {request.RouteKey}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        await PublishOutcomeAsync(operationId, OperationState.Succeeded,
            $"Created route {request.RouteKey}.", cancellationToken);
        _searchRefresh.RequestRefresh();

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

    [LoggerMessage(LogLevel.Trace, "Creating API Gateway v2 route {RouteKey} for {ApiId}.")]
    private partial void LogHandling(string routeKey, string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 create route handled.")]
    private partial void LogHandled();
}
