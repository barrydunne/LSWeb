using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.SendSqsMessage;

/// <summary>
/// Send a message to an SQS queue, optionally with custom attributes and, for FIFO queues, a
/// message group id and deduplication id.
/// </summary>
/// <param name="QueueName">The name of the queue to send the message to.</param>
/// <param name="Body">The message body.</param>
/// <param name="MessageAttributes">Custom string message attributes to attach to the message.</param>
/// <param name="MessageGroupId">The FIFO message group id; required for FIFO queues, otherwise ignored.</param>
/// <param name="MessageDeduplicationId">The FIFO deduplication id; optional.</param>
public record SendSqsMessageCommand(
    string QueueName,
    string Body,
    IReadOnlyDictionary<string, string> MessageAttributes,
    string? MessageGroupId,
    string? MessageDeduplicationId) : ICommand;
