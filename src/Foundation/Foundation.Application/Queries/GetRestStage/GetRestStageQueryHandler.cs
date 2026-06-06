using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGateway;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetRestStage;

internal sealed partial class GetRestStageQueryHandler
    : IQueryHandler<GetRestStageQuery, GetRestStageQueryResult>
{
    private readonly IApiGatewayClient _client;
    private readonly ILogger _logger;

    public GetRestStageQueryHandler(
        IApiGatewayClient client, ILogger<GetRestStageQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetRestStageQueryResult>> Handle(
        GetRestStageQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.StageName, request.RestApiId);
        var stage = await _client.GetStageAsync(
            request.RestApiId, request.StageName, cancellationToken);
        LogHandled(stage.IsSuccess);

        if (!stage.IsSuccess)
        {
            Result<GetRestStageQueryResult> failure = stage.Error!.Value;
            return failure;
        }

        return new GetRestStageQueryResult(stage.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Reading API Gateway REST API stage {StageName} of {RestApiId}.")]
    private partial void LogHandling(string stageName, string restApiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway REST API stage read handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
