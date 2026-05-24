using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Foundation.Application.Queries.FilterLogEvents;
using Foundation.Domain.CloudWatchLogs;
using MediatR;
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
    private static readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(2);

    private readonly StreamSessionManager _sessions;
    private readonly ISender _sender;
    private readonly ILogger _logger;

    public StreamHub(StreamSessionManager sessions, ISender sender, ILogger<StreamHub> logger)
    {
        _sessions = sessions;
        _sender = sender;
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

    /// <summary>
    /// Live-tails a log group, polling CloudWatch Logs for new events that match the optional filter
    /// pattern and streaming each new event to the calling client until the subscription is cancelled.
    /// </summary>
    /// <param name="logGroupName">The name of the log group to tail.</param>
    /// <param name="filterPattern">The CloudWatch Logs filter pattern, or empty for no filter.</param>
    /// <param name="cancellationToken">A token raised when the client disposes the stream.</param>
    /// <returns>An asynchronous stream of new log events.</returns>
    public async IAsyncEnumerable<LogEvent> TailLogGroup(
        string logGroupName,
        string? filterPattern,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        LogTailStarted(logGroupName);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var startTime = DateTimeOffset.UtcNow.AddMinutes(-1);

        while (!cancellationToken.IsCancellationRequested)
        {
            var result = await _sender.Send(
                new FilterLogEventsQuery(logGroupName, filterPattern, startTime, 100), cancellationToken);

            if (result.IsSuccess)
            {
                foreach (var logEvent in result.Value.Events)
                {
                    var key = $"{logEvent.Timestamp.ToUnixTimeMilliseconds()}:{logEvent.Message}";
                    if (!seen.Add(key))
                        continue;

                    if (logEvent.Timestamp > startTime)
                        startTime = logEvent.Timestamp;

                    yield return logEvent;
                }
            }

            await Task.Delay(_pollInterval, cancellationToken);
        }

        LogTailStopped(logGroupName);
    }

    [LoggerMessage(LogLevel.Trace, "Streaming client {ConnectionId} connected.")]
    private partial void LogClientConnected(string connectionId);

    [LoggerMessage(LogLevel.Trace, "Streaming client {ConnectionId} disconnected.")]
    private partial void LogClientDisconnected(string connectionId);

    [LoggerMessage(LogLevel.Trace, "Live tail started for {LogGroupName}.")]
    private partial void LogTailStarted(string logGroupName);

    [LoggerMessage(LogLevel.Trace, "Live tail stopped for {LogGroupName}.")]
    private partial void LogTailStopped(string logGroupName);
}
