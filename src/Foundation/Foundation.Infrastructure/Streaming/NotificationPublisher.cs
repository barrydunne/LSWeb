using System.Diagnostics.CodeAnalysis;
using Foundation.Application.Streaming;
using Foundation.Domain.Streaming;
using Microsoft.AspNetCore.SignalR;

namespace Foundation.Infrastructure.Streaming;

/// <summary>
/// Publishes notifications to every connected client over the SignalR <see cref="StreamHub"/>.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
internal sealed class NotificationPublisher : INotificationPublisher
{
    /// <summary>
    /// The name of the client-side handler invoked to deliver a notification.
    /// </summary>
    public const string ClientMethod = "notification";

    private readonly IHubContext<StreamHub> _hubContext;

    public NotificationPublisher(IHubContext<StreamHub> hubContext)
        => _hubContext = hubContext;

    /// <inheritdoc />
    public Task PublishAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);
        return _hubContext.Clients.All.SendAsync(ClientMethod, notification, cancellationToken);
    }
}
