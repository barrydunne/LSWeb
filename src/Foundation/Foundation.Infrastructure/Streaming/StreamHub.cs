using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Foundation.Infrastructure.Streaming;

/// <summary>
/// The SignalR hub that connected clients subscribe to in order to receive real-time
/// operation-feedback notifications. Connection lifecycle is tracked via the
/// <see cref="StreamSessionManager"/>.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
internal sealed partial class StreamHub : Hub
{
    private readonly StreamSessionManager _sessions;
    private readonly ILogger _logger;

    public StreamHub(StreamSessionManager sessions, ILogger<StreamHub> logger)
    {
        _sessions = sessions;
        _logger = logger;
    }

    /// <inheritdoc />
    public override Task OnConnectedAsync()
    {
        _sessions.Add(Context.ConnectionId);
        LogClientConnected(Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    /// <inheritdoc />
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _sessions.Remove(Context.ConnectionId);
        LogClientDisconnected(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    [LoggerMessage(LogLevel.Trace, "Streaming client {ConnectionId} connected.")]
    private partial void LogClientConnected(string connectionId);

    [LoggerMessage(LogLevel.Trace, "Streaming client {ConnectionId} disconnected.")]
    private partial void LogClientDisconnected(string connectionId);
}
