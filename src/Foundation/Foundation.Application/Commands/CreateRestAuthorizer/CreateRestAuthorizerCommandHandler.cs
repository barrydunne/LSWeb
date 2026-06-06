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

namespace Foundation.Application.Commands.CreateRestAuthorizer;

internal sealed partial class CreateRestAuthorizerCommandHandler
    : ICommandHandler<CreateRestAuthorizerCommand, string>
{
    private const string OperationName = "apigateway-create-authorizer";

    private readonly IApiGatewayClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public CreateRestAuthorizerCommandHandler(
        IApiGatewayClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<CreateRestAuthorizerCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(
        CreateRestAuthorizerCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.Name, request.RestApiId);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Creating authorizer {request.Name}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var specification = new RestAuthorizerSpecification(
            request.RestApiId,
            null,
            request.Name,
            request.Type,
            request.ProviderARNs,
            request.IdentitySource);
        var result = await _client.CreateAuthorizerAsync(specification, cancellationToken);
        if (!result.IsSuccess)
        {
            var failure = $"Failed to create authorizer {request.Name}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        await PublishOutcomeAsync(operationId, OperationState.Succeeded,
            $"Created authorizer {request.Name}.", cancellationToken);
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

    [LoggerMessage(LogLevel.Trace, "Creating API Gateway REST API authorizer {Name} on {RestApiId}.")]
    private partial void LogHandling(string name, string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway create REST API authorizer handled.")]
    private partial void LogHandled();
}
