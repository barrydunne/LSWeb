using Foundation.Domain.Streaming;

namespace Foundation.Application.Streaming;

/// <summary>
/// Publishes operation-lifecycle <see cref="Notification"/> messages to connected clients. The
/// application layer raises notifications through this abstraction without depending on the
/// underlying real-time transport.
/// </summary>
public interface INotificationPublisher
{
    /// <summary>
    /// Push a notification to all connected clients.
    /// </summary>
    /// <param name="notification">The notification to broadcast.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes when the notification has been dispatched.</returns>
    Task PublishAsync(Notification notification, CancellationToken cancellationToken = default);
}
