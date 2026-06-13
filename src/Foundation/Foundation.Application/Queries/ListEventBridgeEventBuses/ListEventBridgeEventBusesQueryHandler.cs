using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.EventBridge;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListEventBridgeEventBuses;

internal sealed partial class ListEventBridgeEventBusesQueryHandler
    : IQueryHandler<ListEventBridgeEventBusesQuery, ListEventBridgeEventBusesQueryResult>
{
    private readonly IEventBridgeClient _client;
    private readonly ILogger _logger;

    public ListEventBridgeEventBusesQueryHandler(
        IEventBridgeClient client, ILogger<ListEventBridgeEventBusesQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListEventBridgeEventBusesQueryResult>> Handle(
        ListEventBridgeEventBusesQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var buses = await _client.ListEventBusesAsync(cancellationToken);
        LogHandled(buses.IsSuccess);

        if (!buses.IsSuccess)
        {
            Result<ListEventBridgeEventBusesQueryResult> failure = buses.Error!.Value;
            return failure;
        }

        return new ListEventBridgeEventBusesQueryResult(buses.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing EventBridge event buses.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "EventBridge event bus list handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
