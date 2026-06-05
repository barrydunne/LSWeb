using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.EventBridge;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetScheduledRule;

internal sealed partial class GetScheduledRuleQueryHandler
    : IQueryHandler<GetScheduledRuleQuery, GetScheduledRuleQueryResult>
{
    private readonly IEventBridgeClient _client;
    private readonly ILogger _logger;

    public GetScheduledRuleQueryHandler(
        IEventBridgeClient client, ILogger<GetScheduledRuleQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetScheduledRuleQueryResult>> Handle(
        GetScheduledRuleQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.Name);
        var rule = await _client.DescribeRuleAsync(
            request.Name, request.EventBusName, cancellationToken);
        LogHandled(rule.IsSuccess);

        if (!rule.IsSuccess)
        {
            Result<GetScheduledRuleQueryResult> failure = rule.Error!.Value;
            return failure;
        }

        return new GetScheduledRuleQueryResult(rule.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Describing EventBridge scheduled rule {Rule}.")]
    private partial void LogHandling(string rule);

    [LoggerMessage(LogLevel.Trace, "EventBridge scheduled rule describe handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
