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

namespace Foundation.Application.Commands.CreateRestResource;

internal sealed partial class CreateRestResourceCommandHandler
    : ICommandHandler<CreateRestResourceCommand, string>
{
    private const string OperationName = "apigateway-create-resource";

    private readonly IApiGatewayClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public CreateRestResourceCommandHandler(
        IApiGatewayClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<CreateRestResourceCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(
        CreateRestResourceCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.PathPart, request.RestApiId);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Creating resource {request.PathPart}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var specification = new RestResourceSpecification(
            request.RestApiId,
            request.ParentId,
            request.PathPart);
        var result = await _client.CreateResourceAsync(specification, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to create resource {request.PathPart}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        await PublishOutcomeAsync(operationId, OperationState.Succeeded,
            $"Created resource {request.PathPart}.", cancellationToken);
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

    [LoggerMessage(LogLevel.Trace, "Creating API Gateway REST API resource {PathPart} on {RestApiId}.")]
    private partial void LogHandling(string pathPart, string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway create REST API resource handled.")]
    private partial void LogHandled();
}
