using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.EventBridge;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListScheduledRules;

internal sealed partial class ListScheduledRulesQueryHandler
    : IQueryHandler<ListScheduledRulesQuery, ListScheduledRulesQueryResult>
{
    private readonly IEventBridgeClient _client;
    private readonly ILogger _logger;

    public ListScheduledRulesQueryHandler(
        IEventBridgeClient client, ILogger<ListScheduledRulesQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListScheduledRulesQueryResult>> Handle(
        ListScheduledRulesQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var rules = await _client.ListRulesAsync(cancellationToken);
        LogHandled(rules.IsSuccess);

        if (!rules.IsSuccess)
        {
            Result<ListScheduledRulesQueryResult> failure = rules.Error!.Value;
            return failure;
        }

        var scheduled = rules.Value
            .Where(rule => !string.IsNullOrWhiteSpace(rule.ScheduleExpression))
            .ToList();

        return new ListScheduledRulesQueryResult(scheduled);
    }

    [LoggerMessage(LogLevel.Trace, "Listing EventBridge scheduled rules.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "EventBridge scheduled rule list handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
