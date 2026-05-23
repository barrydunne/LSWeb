using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.CreateSqsQueue;

/// <summary>
/// Create a new SQS queue, either a standard queue or a FIFO queue.
/// </summary>
/// <param name="QueueName">The name of the queue to create. FIFO queue names must end with <c>.fifo</c>.</param>
/// <param name="FifoQueue">Whether to create a FIFO queue rather than a standard queue.</param>
public record CreateSqsQueueCommand(string QueueName, bool FifoQueue) : ICommand;
