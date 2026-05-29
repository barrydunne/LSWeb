namespace Foundation.Domain.Sqs;

/// <summary>
/// The configurable and informational attributes of an SQS queue. The editable values control how
/// the queue handles message visibility, retention and delivery; the informational values describe
/// the queue's identity, type and approximate message counts and cannot be changed.
/// </summary>
/// <param name="VisibilityTimeoutSeconds">The visibility timeout, in seconds, applied when messages are received.</param>
/// <param name="MessageRetentionPeriodSeconds">How long, in seconds, the queue retains a message before discarding it.</param>
/// <param name="DelaySeconds">The delay, in seconds, before a sent message becomes available for retrieval.</param>
/// <param name="ReceiveMessageWaitTimeSeconds">The long-poll wait time, in seconds, used when receiving messages.</param>
/// <param name="MaximumMessageSizeBytes">The maximum message size, in bytes, the queue accepts.</param>
/// <param name="QueueArn">The Amazon Resource Name of the queue; informational only.</param>
/// <param name="FifoQueue">Whether the queue is a FIFO queue; informational only.</param>
/// <param name="ApproximateMessageCount">The approximate number of visible messages available for retrieval; eventually consistent.</param>
/// <param name="ApproximateInFlightCount">The approximate number of in-flight messages (received but not yet deleted); eventually consistent.</param>
/// <param name="ApproximateDelayedCount">The approximate number of messages delayed and not yet available for retrieval; eventually consistent.</param>
public sealed record SqsQueueAttributes(
    int VisibilityTimeoutSeconds,
    int MessageRetentionPeriodSeconds,
    int DelaySeconds,
    int ReceiveMessageWaitTimeSeconds,
    int MaximumMessageSizeBytes,
    string QueueArn,
    bool FifoQueue,
    long ApproximateMessageCount,
    long ApproximateInFlightCount,
    long ApproximateDelayedCount);
