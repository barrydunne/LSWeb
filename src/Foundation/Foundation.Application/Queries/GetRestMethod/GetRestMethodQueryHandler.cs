using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGateway;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetRestMethod;

internal sealed partial class GetRestMethodQueryHandler
    : IQueryHandler<GetRestMethodQuery, GetRestMethodQueryResult>
{
    private readonly IApiGatewayClient _client;
    private readonly ILogger _logger;

    public GetRestMethodQueryHandler(
        IApiGatewayClient client, ILogger<GetRestMethodQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetRestMethodQueryResult>> Handle(
        GetRestMethodQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.RestApiId, request.ResourceId, request.HttpMethod);
        var method = await _client.GetMethodAsync(
            request.RestApiId, request.ResourceId, request.HttpMethod, cancellationToken);
        LogHandled(method.IsSuccess);

        if (!method.IsSuccess)
        {
            Result<GetRestMethodQueryResult> failure = method.Error!.Value;
            return failure;
        }

        return new GetRestMethodQueryResult(method.Value);
    }

    [LoggerMessage(
        LogLevel.Trace,
        "Reading API Gateway REST API method {HttpMethod} on {ResourceId} of {RestApiId}.")]
    private partial void LogHandling(string restApiId, string resourceId, string httpMethod);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API method read handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
