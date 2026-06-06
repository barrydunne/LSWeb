using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGatewayV2;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetHttpRoute;

internal sealed partial class GetHttpRouteQueryHandler : IQueryHandler<GetHttpRouteQuery, GetHttpRouteQueryResult>
{
    private readonly IApiGatewayV2Client _client;
    private readonly ILogger _logger;

    public GetHttpRouteQueryHandler(IApiGatewayV2Client client, ILogger<GetHttpRouteQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetHttpRouteQueryResult>> Handle(GetHttpRouteQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.ApiId, request.RouteId);
        var route = await _client.GetRouteAsync(request.ApiId, request.RouteId, cancellationToken);
        LogHandled(route.IsSuccess);

        if (!route.IsSuccess)
        {
            Result<GetHttpRouteQueryResult> failure = route.Error!.Value;
            return failure;
        }

        return new GetHttpRouteQueryResult(route.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Reading API Gateway v2 route {RouteId} for {ApiId}.")]
    private partial void LogHandling(string apiId, string routeId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 route read handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
