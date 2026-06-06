using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGatewayV2;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetHttpApi;

internal sealed partial class GetHttpApiQueryHandler : IQueryHandler<GetHttpApiQuery, GetHttpApiQueryResult>
{
    private readonly IApiGatewayV2Client _client;
    private readonly ILogger _logger;

    public GetHttpApiQueryHandler(IApiGatewayV2Client client, ILogger<GetHttpApiQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetHttpApiQueryResult>> Handle(GetHttpApiQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.ApiId);
        var api = await _client.GetApiAsync(request.ApiId, cancellationToken);
        LogHandled(api.IsSuccess);

        if (!api.IsSuccess)
        {
            Result<GetHttpApiQueryResult> failure = api.Error!.Value;
            return failure;
        }

        return new GetHttpApiQueryResult(api.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Reading API Gateway v2 API {ApiId}.")]
    private partial void LogHandling(string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 API read handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
