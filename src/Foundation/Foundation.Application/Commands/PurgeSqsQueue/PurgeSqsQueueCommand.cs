using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.PurgeSqsQueue;

/// <summary>
/// Purge all messages from an SQS queue. This is a destructive action that cannot be undone.
/// </summary>
/// <param name="QueueName">The name of the queue to purge.</param>
public record PurgeSqsQueueCommand(string QueueName) : ICommand;
