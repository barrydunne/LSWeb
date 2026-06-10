using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGateway;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetRestCors;

internal sealed partial class GetRestCorsQueryHandler
    : IQueryHandler<GetRestCorsQuery, GetRestCorsQueryResult>
{
    private readonly IApiGatewayClient _client;
    private readonly ILogger _logger;

    public GetRestCorsQueryHandler(
        IApiGatewayClient client, ILogger<GetRestCorsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetRestCorsQueryResult>> Handle(
        GetRestCorsQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.ResourceId, request.RestApiId);
        var cors = await _client.GetCorsAsync(
            request.RestApiId, request.ResourceId, cancellationToken);
        LogHandled(cors.IsSuccess);

        if (!cors.IsSuccess)
        {
            Result<GetRestCorsQueryResult> failure = cors.Error!.Value;
            return failure;
        }

        return new GetRestCorsQueryResult(cors.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Reading API Gateway REST API CORS policy for resource {ResourceId} of {RestApiId}.")]
    private partial void LogHandling(string resourceId, string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API CORS policy read handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
