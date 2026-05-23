using AspNet.KickStarter.FunctionalResult;
using Foundation.Domain.Sqs;

namespace Foundation.Application.Sqs;

/// <summary>
/// Reads and manages SQS queues on the configured AWS backend. Implementations route through the
/// resilient AWS gateway and never throw across layers, reporting failures as a
/// <see cref="Result{T}"/>.
/// </summary>
public interface ISqsClient
{
    /// <summary>
    /// List the queues visible to the configured backend, including their approximate message counts.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The queues on success; otherwise a failure describing the error.</returns>
    Task<Result<IReadOnlyList<SqsQueue>>> ListQueuesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Create a new queue, either a standard queue or a FIFO queue.
    /// </summary>
    /// <param name="queueName">The name of the queue to create. FIFO queue names must end with <c>.fifo</c>.</param>
    /// <param name="fifoQueue">Whether to create a FIFO queue rather than a standard queue.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Success when the queue is created; otherwise a failure describing the error.</returns>
    Task<Result> CreateQueueAsync(string queueName, bool fifoQueue, CancellationToken cancellationToken);

    /// <summary>
    /// Delete a queue and all of its messages. The operation cannot be undone.
    /// </summary>
    /// <param name="queueName">The name of the queue to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Success when the queue is deleted; otherwise a failure describing the error.</returns>
    Task<Result> DeleteQueueAsync(string queueName, CancellationToken cancellationToken);

    /// <summary>
    /// Receive messages from a queue, either peeking (visibility-preserving) or consuming them.
    /// </summary>
    /// <param name="queueName">The name of the queue to read from.</param>
    /// <param name="mode">Whether to peek (preserve visibility) or consume the messages.</param>
    /// <param name="maxMessages">The maximum number of messages to return; clamped to the backend limit.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The messages on success; otherwise a failure describing the error.</returns>
    Task<Result<IReadOnlyList<SqsMessage>>> ReceiveMessagesAsync(
        string queueName, SqsPollMode mode, int maxMessages, CancellationToken cancellationToken);

    /// <summary>
    /// Delete a single message from a queue using the receipt handle returned when it was received.
    /// </summary>
    /// <param name="queueName">The name of the queue the message was received from.</param>
    /// <param name="receiptHandle">The receipt handle identifying the message to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Success when the message is deleted; otherwise a failure describing the error.</returns>
    Task<Result> DeleteMessageAsync(string queueName, string receiptHandle, CancellationToken cancellationToken);

    /// <summary>
    /// Purge all messages from a queue. The operation is asynchronous on the backend and cannot be
    /// undone.
    /// </summary>
    /// <param name="queueName">The name of the queue to purge.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Success when the purge is accepted; otherwise a failure describing the error.</returns>
    Task<Result> PurgeQueueAsync(string queueName, CancellationToken cancellationToken);

    /// <summary>
    /// Send a message to a queue, optionally with custom attributes and, for FIFO queues, a message
    /// group id and deduplication id.
    /// </summary>
    /// <param name="queueName">The name of the queue to send the message to.</param>
    /// <param name="body">The message body.</param>
    /// <param name="messageAttributes">Custom string message attributes to attach to the message.</param>
    /// <param name="messageGroupId">The FIFO message group id; required for FIFO queues, otherwise ignored.</param>
    /// <param name="messageDeduplicationId">The FIFO deduplication id; optional.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Success when the message is sent; otherwise a failure describing the error.</returns>
    Task<Result> SendMessageAsync(
        string queueName,
        string body,
        IReadOnlyDictionary<string, string> messageAttributes,
        string? messageGroupId,
        string? messageDeduplicationId,
        CancellationToken cancellationToken);

    /// <summary>
    /// List the SNS topics that publish to a queue, detected from the queue's access policy.
    /// </summary>
    /// <param name="queueName">The name of the queue to inspect.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The SNS subscriptions on success; otherwise a failure describing the error.</returns>
    Task<Result<IReadOnlyList<SqsQueueSubscription>>> GetQueueSubscriptionsAsync(
        string queueName, CancellationToken cancellationToken);

    /// <summary>
    /// Read the configurable and informational attributes of a queue.
    /// </summary>
    /// <param name="queueName">The name of the queue to inspect.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The queue attributes on success; otherwise a failure describing the error.</returns>
    Task<Result<SqsQueueAttributes>> GetQueueAttributesAsync(
        string queueName, CancellationToken cancellationToken);

    /// <summary>
    /// Update the editable attributes of a queue.
    /// </summary>
    /// <param name="queueName">The name of the queue to update.</param>
    /// <param name="attributes">The attribute names and values to apply.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Success when the attributes are applied; otherwise a failure describing the error.</returns>
    Task<Result> SetQueueAttributesAsync(
        string queueName, IReadOnlyDictionary<string, string> attributes, CancellationToken cancellationToken);

    /// <summary>
    /// Read the dead-letter queue relationships of a queue from its redrive policies: the
    /// dead-letter queue it feeds and, when it is itself a dead-letter queue, the source queues
    /// allowed to redrive into it.
    /// </summary>
    /// <param name="queueName">The name of the queue to inspect.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The redrive relationships on success; otherwise a failure describing the error.</returns>
    Task<Result<SqsRedrive>> GetQueueRedriveAsync(string queueName, CancellationToken cancellationToken);

    /// <summary>
    /// Start moving messages from a dead-letter queue back to their source queues using the
    /// backend's message-move task. The operation is asynchronous on the backend.
    /// </summary>
    /// <param name="queueName">The name of the dead-letter queue to redrive messages from.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Success when the redrive task is started; otherwise a failure describing the error.</returns>
    Task<Result> StartMessageRedriveAsync(string queueName, CancellationToken cancellationToken);
}
