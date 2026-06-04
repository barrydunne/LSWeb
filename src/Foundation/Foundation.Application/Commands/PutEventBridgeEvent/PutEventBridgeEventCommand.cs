using AspNet.KickStarter.CQRS.Abstractions.Commands;
using Foundation.Domain.EventBridge;

namespace Foundation.Application.Commands.PutEventBridgeEvent;

/// <summary>
/// Put a single custom event onto an EventBridge bus.
/// </summary>
/// <param name="Source">The source that identifies the application emitting the event.</param>
/// <param name="DetailType">The detail type that describes the kind of event.</param>
/// <param name="Detail">The event detail as a JSON object string.</param>
/// <param name="EventBusName">The target event bus name, or <c>null</c> to use the default bus.</param>
public record PutEventBridgeEventCommand(
    string Source,
    string DetailType,
    string Detail,
    string? EventBusName) : ICommand<EventBridgePutResult>;
