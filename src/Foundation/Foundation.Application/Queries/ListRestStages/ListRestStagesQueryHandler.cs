using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGateway;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListRestStages;

internal sealed partial class ListRestStagesQueryHandler
    : IQueryHandler<ListRestStagesQuery, ListRestStagesQueryResult>
{
    private readonly IApiGatewayClient _client;
    private readonly ILogger _logger;

    public ListRestStagesQueryHandler(
        IApiGatewayClient client, ILogger<ListRestStagesQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListRestStagesQueryResult>> Handle(
        ListRestStagesQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.RestApiId);
        var stages = await _client.ListStagesAsync(request.RestApiId, cancellationToken);
        LogHandled(stages.IsSuccess);

        if (!stages.IsSuccess)
        {
            Result<ListRestStagesQueryResult> failure = stages.Error!.Value;
            return failure;
        }

        return new ListRestStagesQueryResult(stages.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing API Gateway REST API stages for {RestApiId}.")]
    private partial void LogHandling(string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API stages list handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
