using System.Collections.Concurrent;
using Foundation.Domain.Streaming;

namespace Foundation.Infrastructure.Streaming;

/// <summary>
/// Tracks the set of currently connected streaming clients in a thread-safe manner so the rest
/// of the system can observe how many clients are receiving real-time notifications.
/// </summary>
internal sealed class StreamSessionManager
{
    private readonly ConcurrentDictionary<string, StreamSession> _sessions = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets the number of currently connected clients.
    /// </summary>
    public int Count => _sessions.Count;

    /// <summary>
    /// Gets a snapshot of the currently connected sessions.
    /// </summary>
    public IReadOnlyCollection<StreamSession> Sessions => _sessions.Values.ToList();

    /// <summary>
    /// Record a newly connected client.
    /// </summary>
    /// <param name="connectionId">The transport connection identifier for the client.</param>
    public void Add(string connectionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);
        _sessions[connectionId] = new StreamSession(connectionId, DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Remove a disconnected client.
    /// </summary>
    /// <param name="connectionId">The transport connection identifier for the client.</param>
    public void Remove(string connectionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);
        _sessions.TryRemove(connectionId, out _);
    }
}
