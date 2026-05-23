using System.Diagnostics.CodeAnalysis;

namespace Foundation.Domain.Sqs;

/// <summary>
/// A concise view of an SQS queue as it appears in a queue list, including its approximate message
/// counts. The counts are reported by the backend and are eventually consistent.
/// </summary>
/// <param name="Name">The name of the queue, derived from its URL.</param>
/// <param name="Url">The fully-qualified URL used to address the queue.</param>
/// <param name="ApproximateMessageCount">The approximate number of visible messages available for retrieval.</param>
/// <param name="ApproximateInFlightCount">The approximate number of in-flight messages (received but not yet deleted).</param>
/// <param name="ApproximateDelayedCount">The approximate number of messages delayed and not yet available for retrieval.</param>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Queue is the domain noun for an SQS queue, not a collection suffix.")]
public sealed record SqsQueue(
    string Name,
    string Url,
    long ApproximateMessageCount,
    long ApproximateInFlightCount,
    long ApproximateDelayedCount);
