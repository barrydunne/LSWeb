using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGatewayV2;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListHttpRoutes;

internal sealed partial class ListHttpRoutesQueryHandler : IQueryHandler<ListHttpRoutesQuery, ListHttpRoutesQueryResult>
{
    private readonly IApiGatewayV2Client _client;
    private readonly ILogger _logger;

    public ListHttpRoutesQueryHandler(IApiGatewayV2Client client, ILogger<ListHttpRoutesQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListHttpRoutesQueryResult>> Handle(ListHttpRoutesQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.ApiId);
        var routes = await _client.ListRoutesAsync(request.ApiId, cancellationToken);
        LogHandled(routes.IsSuccess);

        if (!routes.IsSuccess)
        {
            Result<ListHttpRoutesQueryResult> failure = routes.Error!.Value;
            return failure;
        }

        return new ListHttpRoutesQueryResult(routes.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing API Gateway v2 routes for {ApiId}.")]
    private partial void LogHandling(string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 route listing handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
