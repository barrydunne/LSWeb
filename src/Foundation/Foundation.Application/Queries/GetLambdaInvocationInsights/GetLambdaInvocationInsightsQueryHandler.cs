using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Lambda;
using Foundation.Domain.Lambda;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetLambdaInvocationInsights;

internal sealed partial class GetLambdaInvocationInsightsQueryHandler : IQueryHandler<GetLambdaInvocationInsightsQuery, GetLambdaInvocationInsightsQueryResult>
{
    private const int DefaultLimit = 200;
    private const int MaxLimit = 1000;

    private readonly ILambdaClient _client;
    private readonly ILogger _logger;

    public GetLambdaInvocationInsightsQueryHandler(ILambdaClient client, ILogger<GetLambdaInvocationInsightsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetLambdaInvocationInsightsQueryResult>> Handle(GetLambdaInvocationInsightsQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.FunctionName);
        var limit = request.Limit <= 0 ? DefaultLimit : Math.Min(request.Limit, MaxLimit);
        var events = await _client.GetRecentLogEventsAsync(request.FunctionName, limit, cancellationToken);
        LogHandled(events.IsSuccess);

        if (!events.IsSuccess)
        {
            Result<GetLambdaInvocationInsightsQueryResult> failure = events.Error!.Value;
            return failure;
        }

        var insights = LambdaInvocationLogParser.Parse(events.Value);
        return new GetLambdaInvocationInsightsQueryResult($"/aws/lambda/{request.FunctionName}", insights);
    }

    [LoggerMessage(LogLevel.Trace, "Deriving Lambda invocation insights for {FunctionName}.")]
    private partial void LogHandling(string functionName);

    [LoggerMessage(LogLevel.Trace, "Lambda invocation insights derivation handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
