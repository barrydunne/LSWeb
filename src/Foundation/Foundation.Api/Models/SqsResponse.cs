namespace Foundation.Api.Models;

/// <summary>
/// The SQS queues available on the configured backend.
/// </summary>
/// <param name="Queues">The queue summaries, ordered as returned by the backend.</param>
public sealed record SqsQueueListResponse(IReadOnlyList<SqsQueueResponse> Queues);

/// <summary>
/// A concise view of an SQS queue as it appears in a queue list, including its approximate message
/// counts. The counts are eventually consistent.
/// </summary>
/// <param name="Name">The name of the queue.</param>
/// <param name="Url">The fully-qualified URL used to address the queue.</param>
/// <param name="ApproximateMessageCount">The approximate number of visible messages available for retrieval.</param>
/// <param name="ApproximateInFlightCount">The approximate number of in-flight messages (received but not yet deleted).</param>
/// <param name="ApproximateDelayedCount">The approximate number of messages delayed and not yet available for retrieval.</param>
public sealed record SqsQueueResponse(
    string Name,
    string Url,
    long ApproximateMessageCount,
    long ApproximateInFlightCount,
    long ApproximateDelayedCount);

/// <summary>
/// The messages returned by polling an SQS queue.
/// </summary>
/// <param name="Messages">The messages, ordered as returned by the backend.</param>
public sealed record SqsMessageListResponse(IReadOnlyList<SqsMessageResponse> Messages);

/// <summary>
/// A single SQS message, including its body and the receipt handle required to delete it.
/// </summary>
/// <param name="MessageId">The backend-assigned identifier for the message.</param>
/// <param name="ReceiptHandle">The handle used to delete the message.</param>
/// <param name="Body">The message body as returned by the backend.</param>
/// <param name="Attributes">The system attributes reported for the message.</param>
/// <param name="MessageAttributes">The custom message attributes, projected to their string values.</param>
public sealed record SqsMessageResponse(
    string MessageId,
    string ReceiptHandle,
    string Body,
    IReadOnlyDictionary<string, string> Attributes,
    IReadOnlyDictionary<string, string> MessageAttributes);

/// <summary>
/// The SNS topics that publish to a queue, detected from the queue's access policy.
/// </summary>
/// <param name="Subscriptions">The subscriptions, in first-seen order.</param>
public sealed record SqsSubscriptionListResponse(IReadOnlyList<SqsSubscriptionResponse> Subscriptions);

/// <summary>
/// A single SNS-to-SQS subscription, used to link a queue to its publishing topic.
/// </summary>
/// <param name="TopicArn">The ARN of the SNS topic that is allowed to send to the queue.</param>
/// <param name="TopicName">The bare topic name derived from the ARN.</param>
public sealed record SqsSubscriptionResponse(string TopicArn, string TopicName);

/// <summary>
/// A request to create a new SQS queue.
/// </summary>
/// <param name="QueueName">The name of the queue to create. FIFO queue names must end with <c>.fifo</c>.</param>
/// <param name="FifoQueue">Whether to create a FIFO queue rather than a standard queue.</param>
public sealed record SqsQueueCreateRequest(string QueueName, bool FifoQueue);

/// <summary>
/// A request to send a message to an SQS queue.
/// </summary>
/// <param name="Body">The message body.</param>
/// <param name="MessageAttributes">Custom string message attributes to attach to the message.</param>
/// <param name="MessageGroupId">The FIFO message group id; required for FIFO queues, otherwise ignored.</param>
/// <param name="MessageDeduplicationId">The FIFO deduplication id; optional.</param>
public sealed record SqsSendMessageRequest(
    string Body,
    IReadOnlyDictionary<string, string>? MessageAttributes,
    string? MessageGroupId,
    string? MessageDeduplicationId);

/// <summary>
/// The configurable and informational attributes of an SQS queue.
/// </summary>
/// <param name="VisibilityTimeoutSeconds">The visibility timeout, in seconds, applied when messages are received.</param>
/// <param name="MessageRetentionPeriodSeconds">How long, in seconds, the queue retains a message before discarding it.</param>
/// <param name="DelaySeconds">The delay, in seconds, before a sent message becomes available for retrieval.</param>
/// <param name="ReceiveMessageWaitTimeSeconds">The long-poll wait time, in seconds, used when receiving messages.</param>
/// <param name="MaximumMessageSizeBytes">The maximum message size, in bytes, the queue accepts.</param>
/// <param name="QueueArn">The Amazon Resource Name of the queue; informational only.</param>
/// <param name="FifoQueue">Whether the queue is a FIFO queue; informational only.</param>
public sealed record SqsQueueAttributesResponse(
    int VisibilityTimeoutSeconds,
    int MessageRetentionPeriodSeconds,
    int DelaySeconds,
    int ReceiveMessageWaitTimeSeconds,
    int MaximumMessageSizeBytes,
    string QueueArn,
    bool FifoQueue);

/// <summary>
/// A request to update the editable attributes of an SQS queue.
/// </summary>
/// <param name="VisibilityTimeoutSeconds">The visibility timeout, in seconds, applied when messages are received.</param>
/// <param name="MessageRetentionPeriodSeconds">How long, in seconds, the queue retains a message before discarding it.</param>
/// <param name="DelaySeconds">The delay, in seconds, before a sent message becomes available for retrieval.</param>
/// <param name="ReceiveMessageWaitTimeSeconds">The long-poll wait time, in seconds, used when receiving messages.</param>
public sealed record SqsQueueAttributesUpdateRequest(
    int VisibilityTimeoutSeconds,
    int MessageRetentionPeriodSeconds,
    int DelaySeconds,
    int ReceiveMessageWaitTimeSeconds);

/// <summary>
/// The dead-letter queue relationships of an SQS queue.
/// </summary>
/// <param name="DeadLetterTarget">The dead-letter queue this queue feeds, or <see langword="null"/> when none is configured.</param>
/// <param name="Sources">The source queues permitted to use this queue as their dead-letter queue, in first-seen order.</param>
public sealed record SqsRedriveResponse(
    SqsRedriveTargetResponse? DeadLetterTarget,
    IReadOnlyList<SqsRedriveSourceResponse> Sources);

/// <summary>
/// The dead-letter queue an SQS queue feeds, with the receive-count threshold that triggers redrive.
/// </summary>
/// <param name="QueueArn">The ARN of the dead-letter queue.</param>
/// <param name="QueueName">The bare dead-letter queue name derived from the ARN.</param>
/// <param name="MaxReceiveCount">The number of receives after which a message is moved to the dead-letter queue.</param>
public sealed record SqsRedriveTargetResponse(string QueueArn, string QueueName, int MaxReceiveCount);

/// <summary>
/// A source queue permitted to use the inspected queue as its dead-letter queue.
/// </summary>
/// <param name="QueueArn">The ARN of the source queue.</param>
/// <param name="QueueName">The bare source queue name derived from the ARN.</param>
public sealed record SqsRedriveSourceResponse(string QueueArn, string QueueName);

