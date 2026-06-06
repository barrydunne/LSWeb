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

namespace Foundation.Application.Commands.CreateRestStage;

internal sealed partial class CreateRestStageCommandHandler
    : ICommandHandler<CreateRestStageCommand, string>
{
    private const string OperationName = "apigateway-create-stage";

    private readonly IApiGatewayClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public CreateRestStageCommandHandler(
        IApiGatewayClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<CreateRestStageCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(
        CreateRestStageCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.StageName, request.RestApiId);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Creating stage {request.StageName}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var specification = new RestStageSpecification(
            request.RestApiId,
            request.StageName,
            request.DeploymentId,
            request.Description,
            request.Variables);
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

    [LoggerMessage(LogLevel.Trace, "Creating API Gateway REST API stage {StageName} on {RestApiId}.")]
    private partial void LogHandling(string stageName, string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway create REST API stage handled.")]
    private partial void LogHandled();
}
