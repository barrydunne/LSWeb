using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGateway;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListRestResources;

internal sealed partial class ListRestResourcesQueryHandler
    : IQueryHandler<ListRestResourcesQuery, ListRestResourcesQueryResult>
{
    private readonly IApiGatewayClient _client;
    private readonly ILogger _logger;

    public ListRestResourcesQueryHandler(
        IApiGatewayClient client, ILogger<ListRestResourcesQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListRestResourcesQueryResult>> Handle(
        ListRestResourcesQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.RestApiId);
        var resources = await _client.ListResourcesAsync(request.RestApiId, cancellationToken);
        LogHandled(resources.IsSuccess);

        if (!resources.IsSuccess)
        {
            Result<ListRestResourcesQueryResult> failure = resources.Error!.Value;
            return failure;
        }

        return new ListRestResourcesQueryResult(resources.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing API Gateway REST API resources for {RestApiId}.")]
    private partial void LogHandling(string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API resources list handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
