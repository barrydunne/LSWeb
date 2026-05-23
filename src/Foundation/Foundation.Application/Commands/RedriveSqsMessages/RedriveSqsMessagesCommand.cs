using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.RedriveSqsMessages;

/// <summary>
/// Start moving messages from a dead-letter queue back to their source queues.
/// </summary>
/// <param name="QueueName">The name of the dead-letter queue to redrive messages from.</param>
public record RedriveSqsMessagesCommand(string QueueName) : ICommand;
