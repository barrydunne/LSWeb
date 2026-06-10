using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.ApiGateway;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.ApiGateway;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.TestInvokeRestMethod;

internal sealed partial class TestInvokeRestMethodCommandHandler
    : ICommandHandler<TestInvokeRestMethodCommand, RestMethodTestInvocationResult>
{
    private const string OperationName = "apigateway-test-invoke-rest-method";

    private readonly IApiGatewayClient _client;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public TestInvokeRestMethodCommandHandler(
        IApiGatewayClient client,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<TestInvokeRestMethodCommandHandler> logger)
    {
        _client = client;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result<RestMethodTestInvocationResult>> Handle(
        TestInvokeRestMethodCommand request,
        CancellationToken cancellationToken)
    {
        LogHandling(request.RestApiId, request.ResourceId, request.HttpMethod);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(
                operationId,
                OperationName,
                OperationState.InProgress,
                $"Testing invocation of {request.HttpMethod} on resource {request.ResourceId} in API {request.RestApiId}.",
                DateTimeOffset.UtcNow),
            cancellationToken);

        var result = await _client.TestInvokeMethodAsync(
            new RestMethodTestInvocationSpecification(
                request.RestApiId,
                request.ResourceId,
                request.HttpMethod,
                request.PathWithQueryString,
                request.Headers,
                request.QueryStringParameters,
                request.Body,
                request.StageVariables),
            cancellationToken);

        if (!result.IsSuccess)
        {
            var failure =
                $"Failed to test invoke {request.HttpMethod} on resource {request.ResourceId} in API {request.RestApiId}: {result.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return result.Error!.Value;
        }

        await PublishOutcomeAsync(
            operationId,
            OperationState.Succeeded,
            $"Test invocation completed with status {result.Value.StatusCode} for {request.HttpMethod} on resource {request.ResourceId}.",
            cancellationToken);

        LogHandled();
        return result.Value;
    }

    private async Task PublishOutcomeAsync(
        string operationId,
        OperationState state,
        string message,
        CancellationToken cancellationToken)
    {
        await _publisher.PublishAsync(
            new Notification(operationId, OperationName, state, message, DateTimeOffset.UtcNow),
            cancellationToken);
        _activityLog.Append(
            new ActivityEntry(operationId, OperationName, state, message, DateTimeOffset.UtcNow));
    }

    [LoggerMessage(
        LogLevel.Trace,
        "Testing invocation for API {RestApiId}, resource {ResourceId}, method {HttpMethod}.")]
    private partial void LogHandling(string restApiId, string resourceId, string httpMethod);

    [LoggerMessage(LogLevel.Trace, "API Gateway test invoke handled.")]
    private partial void LogHandled();
}
