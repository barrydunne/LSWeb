using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.EventBridge;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetEventBridgeRule;

internal sealed partial class GetEventBridgeRuleQueryHandler
    : IQueryHandler<GetEventBridgeRuleQuery, GetEventBridgeRuleQueryResult>
{
    private readonly IEventBridgeClient _client;
    private readonly ILogger _logger;

    public GetEventBridgeRuleQueryHandler(
        IEventBridgeClient client, ILogger<GetEventBridgeRuleQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetEventBridgeRuleQueryResult>> Handle(
        GetEventBridgeRuleQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.Name);
        var rule = await _client.DescribeRuleAsync(
            request.Name, request.EventBusName, cancellationToken);
        LogHandled(rule.IsSuccess);

        if (!rule.IsSuccess)
        {
            Result<GetEventBridgeRuleQueryResult> failure = rule.Error!.Value;
            return failure;
        }

        return new GetEventBridgeRuleQueryResult(rule.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Describing EventBridge rule {Rule}.")]
    private partial void LogHandling(string rule);

    [LoggerMessage(LogLevel.Trace, "EventBridge rule describe handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
