using AspNet.KickStarter.CQRS.Abstractions.Commands;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Activity;
using Foundation.Application.ApiGatewayV2;
using Foundation.Application.Streaming;
using Foundation.Domain.Activity;
using Foundation.Domain.ApiGatewayV2;
using Foundation.Domain.Streaming;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Commands.TestHttpRoute;

internal sealed partial class TestHttpRouteCommandHandler
    : ICommandHandler<TestHttpRouteCommand, HttpRouteInvocationResult>
{
    private const string OperationName = "apigatewayv2-test-invoke-route";
    private const string DefaultStage = "$default";

    private readonly IApiGatewayV2Client _client;
    private readonly IHttpApiRouteInvoker _invoker;
    private readonly INotificationPublisher _publisher;
    private readonly IActivityLog _activityLog;
    private readonly ILogger _logger;

    public TestHttpRouteCommandHandler(
        IApiGatewayV2Client client,
        IHttpApiRouteInvoker invoker,
        INotificationPublisher publisher,
        IActivityLog activityLog,
        ILogger<TestHttpRouteCommandHandler> logger)
    {
        _client = client;
        _invoker = invoker;
        _publisher = publisher;
        _activityLog = activityLog;
        _logger = logger;
    }

    public async Task<Result<HttpRouteInvocationResult>> Handle(
        TestHttpRouteCommand request,
        CancellationToken cancellationToken)
    {
        LogHandling(request.ApiId, request.Method, request.Path);
        var operationId = Guid.NewGuid().ToString();

        await _publisher.PublishAsync(
            new Notification(
                operationId,
                OperationName,
                OperationState.InProgress,
                $"Invoking {request.Method} {request.Path} on API {request.ApiId}.",
                DateTimeOffset.UtcNow),
            cancellationToken);

        var apiResult = await _client.GetApiAsync(request.ApiId, cancellationToken);
        if (!apiResult.IsSuccess)
        {
            var failure =
                $"Failed to resolve the invoke endpoint of API {request.ApiId}: {apiResult.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return apiResult.Error!.Value;
        }

        var endpoint = apiResult.Value.ApiEndpoint;
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            var failure = $"API {request.ApiId} does not expose an invoke endpoint.";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return new Error(failure);
        }

        var requestUri = BuildInvokeUri(endpoint, request.Stage, request.Path);

        var invokeResult = await _invoker.InvokeAsync(
            requestUri, request.Method, request.Token, request.Body, cancellationToken);

        if (!invokeResult.IsSuccess)
        {
            var failure =
                $"Failed to invoke {request.Method} {request.Path} on API {request.ApiId}: {invokeResult.Error!.Value.Message}";
            await PublishOutcomeAsync(operationId, OperationState.Failed, failure, cancellationToken);
            return invokeResult.Error!.Value;
        }

        var outcome = invokeResult.Value.Authorized ? "authorized" : "unauthorized";
        await PublishOutcomeAsync(
            operationId,
            OperationState.Succeeded,
            $"Invocation of {request.Method} {request.Path} on API {request.ApiId} returned status {invokeResult.Value.StatusCode} ({outcome}).",
            cancellationToken);

        LogHandled();
        return invokeResult.Value;
    }

    private static string BuildInvokeUri(string endpoint, string stage, string path)
    {
        var baseUri = endpoint.TrimEnd('/');

        var trimmedStage = stage.Trim();
        var stageSegment = trimmedStage.Length == 0 || string.Equals(trimmedStage, DefaultStage, StringComparison.Ordinal)
            ? string.Empty
            : "/" + trimmedStage.Trim('/');

        var normalizedPath = path.Trim();
        if (normalizedPath.Length == 0)
            normalizedPath = "/";
        else if (!normalizedPath.StartsWith('/'))
            normalizedPath = "/" + normalizedPath;

        return baseUri + stageSegment + normalizedPath;
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
        "Testing invocation of {Method} {Path} on API {ApiId}.")]
    private partial void LogHandling(string apiId, string method, string path);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 route test invoke handled.")]
    private partial void LogHandled();
}
