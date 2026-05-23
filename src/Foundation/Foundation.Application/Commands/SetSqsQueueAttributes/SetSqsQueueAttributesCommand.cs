using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.SetSqsQueueAttributes;

/// <summary>
/// Update the editable attributes of a queue.
/// </summary>
/// <param name="QueueName">The name of the queue to update.</param>
/// <param name="VisibilityTimeoutSeconds">The visibility timeout, in seconds, applied when messages are received.</param>
/// <param name="MessageRetentionPeriodSeconds">How long, in seconds, the queue retains a message before discarding it.</param>
/// <param name="DelaySeconds">The delay, in seconds, before a sent message becomes available for retrieval.</param>
/// <param name="ReceiveMessageWaitTimeSeconds">The long-poll wait time, in seconds, used when receiving messages.</param>
public record SetSqsQueueAttributesCommand(
    string QueueName,
    int VisibilityTimeoutSeconds,
    int MessageRetentionPeriodSeconds,
    int DelaySeconds,
    int ReceiveMessageWaitTimeSeconds) : ICommand;
