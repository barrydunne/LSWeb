namespace Foundation.Domain.Streaming;

/// <summary>
/// A single connected client streaming session. Used to track which clients are currently
/// receiving real-time notifications.
/// </summary>
/// <param name="ConnectionId">The transport connection identifier for the client.</param>
/// <param name="ConnectedAt">When the client connected.</param>
public sealed record StreamSession(
    string ConnectionId,
    DateTimeOffset ConnectedAt);
