using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Sqs;

namespace Foundation.Application.Queries.GetSqsQueueRedrive;

/// <summary>
/// Read the dead-letter queue relationships of a queue from its redrive policies.
/// </summary>
/// <param name="QueueName">The name of the queue to inspect.</param>
public record GetSqsQueueRedriveQuery(string QueueName) : IQuery<GetSqsQueueRedriveQueryResult>;

/// <summary>
/// The dead-letter queue relationships of a queue.
/// </summary>
/// <param name="Redrive">The dead-letter target and source queues for the queue.</param>
public record GetSqsQueueRedriveQueryResult(SqsRedrive Redrive);
