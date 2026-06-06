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

namespace Foundation.Application.Commands.CreateHttpAuthorizer;

internal sealed partial class CreateHttpAuthorizerCommandHandler : ICommandHandler<CreateHttpAuthorizerCommand, string>
{
    private const string OperationName = "apigatewayv2-create-authorizer";

    private readonly IApiGatewayV2Client _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ISearchRefreshTrigger _searchRefresh;
    private readonly ILogger _logger;

    public CreateHttpAuthorizerCommandHandler(
        IApiGatewayV2Client client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ISearchRefreshTrigger searchRefresh,
        ILogger<CreateHttpAuthorizerCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _searchRefresh = searchRefresh;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(CreateHttpAuthorizerCommand request, CancellationToken cancellationToken)
    {
        LogHandling(request.Name, request.ApiId);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, OperationState.InProgress,
                $"Creating authorizer {request.Name}.", DateTimeOffset.UtcNow),
            cancellationToken);

        var specification = new HttpAuthorizerSpecification(
            request.ApiId,
            null,
            request.Name,
            request.AuthorizerType,
            request.IdentitySource,
            request.JwtIssuer,
            request.JwtAudience);
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

    [LoggerMessage(LogLevel.Trace, "Creating API Gateway v2 authorizer {Name} for {ApiId}.")]
    private partial void LogHandling(string name, string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 create authorizer handled.")]
    private partial void LogHandled();
}
