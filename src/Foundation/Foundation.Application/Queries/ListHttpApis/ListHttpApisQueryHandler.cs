using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGatewayV2;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListHttpApis;

internal sealed partial class ListHttpApisQueryHandler : IQueryHandler<ListHttpApisQuery, ListHttpApisQueryResult>
{
    private readonly IApiGatewayV2Client _client;
    private readonly ILogger _logger;

    public ListHttpApisQueryHandler(IApiGatewayV2Client client, ILogger<ListHttpApisQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListHttpApisQueryResult>> Handle(ListHttpApisQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var apis = await _client.ListApisAsync(cancellationToken);
        LogHandled(apis.IsSuccess);

        if (!apis.IsSuccess)
        {
            Result<ListHttpApisQueryResult> failure = apis.Error!.Value;
            return failure;
        }

        return new ListHttpApisQueryResult(apis.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing API Gateway v2 APIs.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 API listing handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
