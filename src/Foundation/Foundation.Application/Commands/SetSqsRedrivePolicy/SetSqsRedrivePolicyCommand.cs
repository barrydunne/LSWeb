using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.SetSqsRedrivePolicy;

/// <summary>
/// Configure the redrive (dead-letter queue) policy of a source SQS queue.
/// </summary>
/// <param name="QueueName">The name of the source queue to configure.</param>
/// <param name="DeadLetterTargetArn">The Amazon Resource Name of the dead-letter queue.</param>
/// <param name="MaxReceiveCount">The number of receives after which a message is moved to the dead-letter queue.</param>
public record SetSqsRedrivePolicyCommand(
    string QueueName, string DeadLetterTargetArn, int MaxReceiveCount) : ICommand;
