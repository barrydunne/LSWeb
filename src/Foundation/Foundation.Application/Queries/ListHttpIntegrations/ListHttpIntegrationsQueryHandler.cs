using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGatewayV2;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListHttpIntegrations;

internal sealed partial class ListHttpIntegrationsQueryHandler : IQueryHandler<ListHttpIntegrationsQuery, ListHttpIntegrationsQueryResult>
{
    private readonly IApiGatewayV2Client _client;
    private readonly ILogger _logger;

    public ListHttpIntegrationsQueryHandler(IApiGatewayV2Client client, ILogger<ListHttpIntegrationsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListHttpIntegrationsQueryResult>> Handle(ListHttpIntegrationsQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.ApiId);
        var integrations = await _client.ListIntegrationsAsync(request.ApiId, cancellationToken);
        LogHandled(integrations.IsSuccess);

        if (!integrations.IsSuccess)
        {
            Result<ListHttpIntegrationsQueryResult> failure = integrations.Error!.Value;
            return failure;
        }

        return new ListHttpIntegrationsQueryResult(integrations.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing API Gateway v2 integrations for {ApiId}.")]
    private partial void LogHandling(string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 integration listing handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
