using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Sqs;

namespace Foundation.Application.Queries.GetSqsQueueAttributes;

/// <summary>
/// Read the configurable and informational attributes of a queue.
/// </summary>
/// <param name="QueueName">The name of the queue to inspect.</param>
public record GetSqsQueueAttributesQuery(string QueueName) : IQuery<GetSqsQueueAttributesQueryResult>;

/// <summary>
/// The attributes of a queue.
/// </summary>
/// <param name="Attributes">The configurable and informational queue attributes.</param>
public record GetSqsQueueAttributesQueryResult(SqsQueueAttributes Attributes);
