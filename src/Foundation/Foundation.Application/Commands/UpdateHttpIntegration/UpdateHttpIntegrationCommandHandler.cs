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

namespace Foundation.Application.Commands.UpdateHttpIntegration;

internal sealed partial class UpdateHttpIntegrationCommandHandler : ICommandHandler<UpdateHttpIntegrationCommand>
{
    private const string OperationName = "apigatewayv2-update-integration";

    private readonly IApiGatewayV2Client _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public UpdateHttpIntegrationCommandHandler(
        IApiGatewayV2Client client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<UpdateHttpIntegrationCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateHttpIntegrationCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.IntegrationId, request.ApiId);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Updating integration {request.IntegrationId}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var specification = new HttpIntegrationSpecification(
            request.ApiId,
            request.IntegrationType,
            request.IntegrationMethod,
            request.IntegrationUri,
            request.PayloadFormatVersion,
            request.Description,
            request.IntegrationId);
        var result = await _client.UpdateIntegrationAsync(specification, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to update integration {request.IntegrationId}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        await PublishOutcomeAsync(operationId, OperationState.Succeeded,
            $"Updated integration {request.IntegrationId}.", cancellationToken);
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

    [LoggerMessage(LogLevel.Trace, "Updating API Gateway v2 integration {IntegrationId} for {ApiId}.")]
    private partial void LogHandling(string integrationId, string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 update integration handled.")]
    private partial void LogHandled();
}
