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

namespace Foundation.Application.Commands.UpdateHttpApi;

internal sealed partial class UpdateHttpApiCommandHandler : ICommandHandler<UpdateHttpApiCommand>
{
    private const string OperationName = "apigatewayv2-update-api";

    private readonly IApiGatewayV2Client _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public UpdateHttpApiCommandHandler(
        IApiGatewayV2Client client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<UpdateHttpApiCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateHttpApiCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.ApiId);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Updating {request.ApiId}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var specification = new HttpApiSpecification(
            request.ApiId,
            request.Name,
            request.ProtocolType,
            request.Description,
            request.Version,
            request.RouteSelectionExpression,
            request.CorsConfiguration);
        var result = await _client.UpdateApiAsync(specification, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to update {request.ApiId}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        await PublishOutcomeAsync(operationId, OperationState.Succeeded,
            $"Updated {request.ApiId}.", cancellationToken);
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

    [LoggerMessage(LogLevel.Trace, "Updating API Gateway v2 API {ApiId}.")]
    private partial void LogHandling(string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 update API handled.")]
    private partial void LogHandled();
}
