using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.EventBridge;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListEventBridgeTargets;

internal sealed partial class ListEventBridgeTargetsQueryHandler
    : IQueryHandler<ListEventBridgeTargetsQuery, ListEventBridgeTargetsQueryResult>
{
    private readonly IEventBridgeClient _client;
    private readonly ILogger _logger;

    public ListEventBridgeTargetsQueryHandler(
        IEventBridgeClient client, ILogger<ListEventBridgeTargetsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListEventBridgeTargetsQueryResult>> Handle(
        ListEventBridgeTargetsQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.RuleName);
        var targets = await _client.ListTargetsByRuleAsync(request.RuleName, cancellationToken);
        LogHandled(targets.IsSuccess);

        if (!targets.IsSuccess)
        {
            Result<ListEventBridgeTargetsQueryResult> failure = targets.Error!.Value;
            return failure;
        }

        return new ListEventBridgeTargetsQueryResult(targets.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing EventBridge targets for rule {RuleName}.")]
    private partial void LogHandling(string ruleName);

    [LoggerMessage(LogLevel.Trace, "EventBridge target list handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
