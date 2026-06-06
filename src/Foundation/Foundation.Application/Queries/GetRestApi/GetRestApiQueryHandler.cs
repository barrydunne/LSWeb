using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGateway;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetRestApi;

internal sealed partial class GetRestApiQueryHandler
    : IQueryHandler<GetRestApiQuery, GetRestApiQueryResult>
{
    private readonly IApiGatewayClient _client;
    private readonly ILogger _logger;

    public GetRestApiQueryHandler(
        IApiGatewayClient client, ILogger<GetRestApiQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetRestApiQueryResult>> Handle(
        GetRestApiQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.RestApiId);
        var restApi = await _client.GetRestApiAsync(request.RestApiId, cancellationToken);
        LogHandled(restApi.IsSuccess);

        if (!restApi.IsSuccess)
        {
            Result<GetRestApiQueryResult> failure = restApi.Error!.Value;
            return failure;
        }

        return new GetRestApiQueryResult(restApi.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Reading API Gateway REST API {RestApiId}.")]
    private partial void LogHandling(string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API read handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
