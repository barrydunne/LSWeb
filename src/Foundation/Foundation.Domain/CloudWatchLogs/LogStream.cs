using System.Diagnostics.CodeAnalysis;

namespace Foundation.Domain.CloudWatchLogs;

/// <summary>
/// A concise view of a CloudWatch log stream within a log group.
/// </summary>
/// <param name="Name">The name of the log stream.</param>
/// <param name="LastEventTimestamp">The time of the most recent event in the stream, if reported by the backend.</param>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "LogStream is the domain noun for a CloudWatch log stream, not a System.IO.Stream.")]
public sealed record LogStream(
    string Name,
    DateTimeOffset? LastEventTimestamp);
