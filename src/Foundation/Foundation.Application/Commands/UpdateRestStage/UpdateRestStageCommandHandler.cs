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

namespace Foundation.Application.Commands.UpdateRestStage;

internal sealed partial class UpdateRestStageCommandHandler
    : ICommandHandler<UpdateRestStageCommand>
{
    private const string OperationName = "apigateway-update-stage";

    private readonly IApiGatewayClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public UpdateRestStageCommandHandler(
        IApiGatewayClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<UpdateRestStageCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result> Handle(
        UpdateRestStageCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.StageName, request.RestApiId);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Updating stage {request.StageName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var specification = new RestStageSpecification(
            request.RestApiId,
            request.StageName,
            string.Empty,
            request.Description,
            request.Variables);
        var result = await _client.UpdateStageAsync(specification, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to update stage {request.StageName}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        await PublishOutcomeAsync(operationId, OperationState.Succeeded,
            $"Updated stage {request.StageName}.", cancellationToken);
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

    [LoggerMessage(LogLevel.Trace, "Updating API Gateway REST API stage {StageName} on {RestApiId}.")]
    private partial void LogHandling(string stageName, string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway update REST API stage handled.")]
    private partial void LogHandled();
}
