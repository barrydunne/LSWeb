using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.ApiGateway;
using Foundation.Application.Search;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.ApiGateway;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.ConfigureRestCors;

internal sealed partial class ConfigureRestCorsCommandHandler
    : ICommandHandler<ConfigureRestCorsCommand>
{
    private const string OperationName = "apigateway-configure-cors";

    private readonly IApiGatewayClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public ConfigureRestCorsCommandHandler(
        IApiGatewayClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<ConfigureRestCorsCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(
        ConfigureRestCorsCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.ResourceId, request.RestApiId);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Configuring CORS on resource {request.ResourceId}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var specification = new RestCorsSpecification(
            request.RestApiId,
            request.ResourceId,
            request.AllowOrigins,
            request.AllowMethods,
            request.AllowHeaders);
        var result = await _client.ConfigureCorsAsync(specification, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to configure CORS on resource {request.ResourceId}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        await PublishOutcomeAsync(operationId, OperationState.Succeeded,
            $"Configured CORS on resource {request.ResourceId}.", cancellationToken);
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

    [LoggerMessage(LogLevel.Trace, "Configuring API Gateway REST API CORS policy on resource {ResourceId} of {RestApiId}.")]
    private partial void LogHandling(string resourceId, string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway configure REST API CORS handled.")]
    private partial void LogHandled();
}
