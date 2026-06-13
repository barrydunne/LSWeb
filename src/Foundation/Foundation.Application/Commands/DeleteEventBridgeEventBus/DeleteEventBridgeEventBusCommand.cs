using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteEventBridgeEventBus;

/// <summary>
/// Delete a custom EventBridge event bus.
/// </summary>
/// <param name="Name">The name of the event bus to delete.</param>
public record DeleteEventBridgeEventBusCommand(string Name) : ICommand;
