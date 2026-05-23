using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.DeleteSqsQueue;

/// <summary>
/// Delete an SQS queue and all of its messages.
/// </summary>
/// <param name="QueueName">The name of the queue to delete.</param>
public record DeleteSqsQueueCommand(string QueueName) : ICommand;
