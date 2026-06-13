using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.EventBridge;

namespace Foundation.Application.Queries.ListEventBridgeEventBuses;

/// <summary>
/// List the event buses available on the backend, including the default bus and any custom buses.
/// </summary>
public record ListEventBridgeEventBusesQuery : IQuery<ListEventBridgeEventBusesQueryResult>;

/// <summary>
/// The event buses available on the backend.
/// </summary>
/// <param name="Buses">The event buses, ordered as returned by the backend.</param>
public record ListEventBridgeEventBusesQueryResult(IReadOnlyList<EventBridgeEventBus> Buses);
