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

namespace Foundation.Application.Commands.CreateHttpStage;

internal sealed partial class CreateHttpStageCommandHandler : ICommandHandler<CreateHttpStageCommand, string>
{
    private const string OperationName = "apigatewayv2-create-stage";

    private readonly IApiGatewayV2Client _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public CreateHttpStageCommandHandler(
        IApiGatewayV2Client client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<CreateHttpStageCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(CreateHttpStageCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.StageName, request.ApiId);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Creating stage {request.StageName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var specification = new HttpStageSpecification(
            request.ApiId,
            request.StageName,
            request.AutoDeploy,
            request.Description,
            request.DefaultRouteThrottlingBurstLimit,
            request.DefaultRouteThrottlingRateLimit,
            request.StageVariables);
        var result = await _client.CreateStageAsync(specification, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to create stage {request.StageName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        await PublishOutcomeAsync(operationId, OperationState.Succeeded,
            $"Created stage {request.StageName}.", cancellationToken);
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

    [LoggerMessage(LogLevel.Trace, "Creating API Gateway v2 stage {StageName} for {ApiId}.")]
    private partial void LogHandling(string stageName, string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 create stage handled.")]
    private partial void LogHandled();
}
