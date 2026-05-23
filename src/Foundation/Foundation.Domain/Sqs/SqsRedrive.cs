namespace Foundation.Domain.Sqs;

/// <summary>
/// The dead-letter queue relationships of an SQS queue, read from its <c>RedrivePolicy</c> and
/// <c>RedriveAllowPolicy</c> attributes. Lets the UI link a queue to the dead-letter queue it feeds
/// and, when the queue is itself a dead-letter queue, to the source queues allowed to redrive into it.
/// </summary>
/// <param name="DeadLetterTarget">The dead-letter queue this queue sends failed messages to, or <see langword="null"/> when none is configured.</param>
/// <param name="Sources">The source queues permitted to use this queue as their dead-letter queue, in first-seen order; empty when the queue is not a dead-letter queue or when all sources are allowed.</param>
public sealed record SqsRedrive(SqsRedriveTarget? DeadLetterTarget, IReadOnlyList<SqsRedriveSource> Sources);

/// <summary>
/// The dead-letter queue an SQS queue feeds, with the receive-count threshold that triggers redrive.
/// </summary>
/// <param name="QueueArn">The ARN of the dead-letter queue.</param>
/// <param name="QueueName">The bare dead-letter queue name derived from the ARN.</param>
/// <param name="MaxReceiveCount">The number of receives after which a message is moved to the dead-letter queue.</param>
public sealed record SqsRedriveTarget(string QueueArn, string QueueName, int MaxReceiveCount);

/// <summary>
/// A source queue permitted to use the inspected queue as its dead-letter queue.
/// </summary>
/// <param name="QueueArn">The ARN of the source queue.</param>
/// <param name="QueueName">The bare source queue name derived from the ARN.</param>
public sealed record SqsRedriveSource(string QueueArn, string QueueName);
