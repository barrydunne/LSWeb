using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGatewayV2;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListHttpStages;

internal sealed partial class ListHttpStagesQueryHandler : IQueryHandler<ListHttpStagesQuery, ListHttpStagesQueryResult>
{
    private readonly IApiGatewayV2Client _client;
    private readonly ILogger _logger;

    public ListHttpStagesQueryHandler(IApiGatewayV2Client client, ILogger<ListHttpStagesQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListHttpStagesQueryResult>> Handle(ListHttpStagesQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.ApiId);
        var stages = await _client.ListStagesAsync(request.ApiId, cancellationToken);
        LogHandled(stages.IsSuccess);

        if (!stages.IsSuccess)
        {
            Result<ListHttpStagesQueryResult> failure = stages.Error!.Value;
            return failure;
        }

        return new ListHttpStagesQueryResult(stages.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing API Gateway v2 stages for {ApiId}.")]
    private partial void LogHandling(string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 stage listing handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
