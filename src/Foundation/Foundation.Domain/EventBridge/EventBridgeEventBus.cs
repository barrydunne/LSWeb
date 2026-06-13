namespace Foundation.Domain.EventBridge;

/// <summary>
/// A concise view of an EventBridge event bus.
/// </summary>
/// <param name="Name">The event bus name, unique within the account and region.</param>
/// <param name="Arn">The Amazon Resource Name that uniquely identifies the event bus.</param>
public sealed record EventBridgeEventBus(string Name, string Arn);
