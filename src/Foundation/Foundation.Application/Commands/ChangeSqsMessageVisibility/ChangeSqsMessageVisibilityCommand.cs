using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.ChangeSqsMessageVisibility;

/// <summary>
/// Override the visibility timeout of a single in-flight SQS message.
/// </summary>
/// <param name="QueueName">The name of the queue the message was received from.</param>
/// <param name="ReceiptHandle">The receipt handle identifying the message to update.</param>
/// <param name="VisibilityTimeoutSeconds">The new visibility timeout, in seconds.</param>
public record ChangeSqsMessageVisibilityCommand(
    string QueueName, string ReceiptHandle, int VisibilityTimeoutSeconds) : ICommand;
