using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteSqsMessage;

/// <summary>
/// Delete a single message from an SQS queue using its receipt handle.
/// </summary>
/// <param name="QueueName">The name of the queue the message was received from.</param>
/// <param name="ReceiptHandle">The receipt handle identifying the message to delete.</param>
public record DeleteSqsMessageCommand(string QueueName, string ReceiptHandle) : ICommand;
