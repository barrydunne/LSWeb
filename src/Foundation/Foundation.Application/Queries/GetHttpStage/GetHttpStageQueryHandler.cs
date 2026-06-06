using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.ApiGatewayV2;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetHttpStage;

internal sealed partial class GetHttpStageQueryHandler : IQueryHandler<GetHttpStageQuery, GetHttpStageQueryResult>
{
    private readonly IApiGatewayV2Client _client;
    private readonly ILogger _logger;

    public GetHttpStageQueryHandler(IApiGatewayV2Client client, ILogger<GetHttpStageQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetHttpStageQueryResult>> Handle(GetHttpStageQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.StageName, request.ApiId);
        var stage = await _client.GetStageAsync(request.ApiId, request.StageName, cancellationToken);
        LogHandled(stage.IsSuccess);

        if (!stage.IsSuccess)
        {
            Result<GetHttpStageQueryResult> failure = stage.Error!.Value;
            return failure;
        }

        return new GetHttpStageQueryResult(stage.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Reading API Gateway v2 stage {StageName} for {ApiId}.")]
    private partial void LogHandling(string stageName, string apiId);

    [LoggerMessage(LogLevel.Trace, "API Gateway v2 stage read handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
