using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.EventBridge;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListEventBridgeRules;

internal sealed partial class ListEventBridgeRulesQueryHandler
    : IQueryHandler<ListEventBridgeRulesQuery, ListEventBridgeRulesQueryResult>
{
    private readonly IEventBridgeClient _client;
    private readonly ILogger _logger;

    public ListEventBridgeRulesQueryHandler(
        IEventBridgeClient client, ILogger<ListEventBridgeRulesQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListEventBridgeRulesQueryResult>> Handle(
        ListEventBridgeRulesQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var rules = await _client.ListRulesAsync(cancellationToken);
        LogHandled(rules.IsSuccess);

        if (!rules.IsSuccess)
        {
            Result<ListEventBridgeRulesQueryResult> failure = rules.Error!.Value;
            return failure;
        }

        return new ListEventBridgeRulesQueryResult(rules.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing EventBridge rules.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "EventBridge rule list handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
