using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGateway;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListRestApis;

internal sealed partial class ListRestApisQueryHandler
    : IQueryHandler<ListRestApisQuery, ListRestApisQueryResult>
{
    private readonly IApiGatewayClient _client;
    private readonly ILogger _logger;

    public ListRestApisQueryHandler(
        IApiGatewayClient client, ILogger<ListRestApisQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListRestApisQueryResult>> Handle(
        ListRestApisQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var restApis = await _client.ListRestApisAsync(cancellationToken);
        LogHandled(restApis.IsSuccess);

        if (!restApis.IsSuccess)
        {
            Result<ListRestApisQueryResult> failure = restApis.Error!.Value;
            return failure;
        }

        return new ListRestApisQueryResult(restApis.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing API Gateway REST APIs.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API list handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
