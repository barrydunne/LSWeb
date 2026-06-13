using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.CreateEventBridgeEventBus;

/// <summary>
/// Create a custom EventBridge event bus.
/// </summary>
/// <param name="Name">The name of the event bus to create.</param>
public record CreateEventBridgeEventBusCommand(string Name) : ICommand;
