using AspNet.KickStarter.FunctionalResult;
using Foundation.Domain.CloudWatchLogs;

namespace Foundation.Application.CloudWatchLogs;

/// <summary>
/// Provides access to AWS CloudWatch Logs: browsing log groups, the streams within a group, and the
/// events within a stream.
/// </summary>
public interface ICloudWatchLogsClient
{
    /// <summary>
    /// Lists the log groups available on the configured backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The log groups, or an error if the backend could not be reached.</returns>
    Task<Result<IReadOnlyList<LogGroup>>> ListLogGroupsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Lists the log streams within a log group, most recently active first.
    /// </summary>
    /// <param name="logGroupName">The name of the log group to inspect.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The log streams, or an error if the backend could not be reached.</returns>
    Task<Result<IReadOnlyList<LogStream>>> ListLogStreamsAsync(string logGroupName, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the most recent events from a log stream.
    /// </summary>
    /// <param name="logGroupName">The name of the log group the stream belongs to.</param>
    /// <param name="logStreamName">The name of the log stream to read from.</param>
    /// <param name="limit">The maximum number of events to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The log events in chronological order, or an error if the backend could not be reached.</returns>
    Task<Result<IReadOnlyList<LogEvent>>> GetLogEventsAsync(
        string logGroupName, string logStreamName, int limit, CancellationToken cancellationToken);

    /// <summary>
    /// Searches the events across every stream in a log group, optionally constrained by a filter
    /// pattern and a start time. Used to power filtered search and live tail.
    /// </summary>
    /// <param name="logGroupName">The name of the log group to search.</param>
    /// <param name="filterPattern">The CloudWatch Logs filter pattern, or <see langword="null"/> for no filter.</param>
    /// <param name="startTime">Only return events at or after this time, or <see langword="null"/> for no lower bound.</param>
    /// <param name="limit">The maximum number of events to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The matching log events in chronological order, or an error if the backend could not be reached.</returns>
    Task<Result<IReadOnlyList<LogEvent>>> FilterLogEventsAsync(
        string logGroupName, string? filterPattern, DateTimeOffset? startTime, int limit, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new, empty log group.
    /// </summary>
    /// <param name="logGroupName">The name of the log group to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the backend could not be reached.</returns>
    Task<Result> CreateLogGroupAsync(string logGroupName, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a log group and all of the streams and events it contains.
    /// </summary>
    /// <param name="logGroupName">The name of the log group to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success result, or an error if the backend could not be reached.</returns>
    Task<Result> DeleteLogGroupAsync(string logGroupName, CancellationToken cancellationToken);
}
