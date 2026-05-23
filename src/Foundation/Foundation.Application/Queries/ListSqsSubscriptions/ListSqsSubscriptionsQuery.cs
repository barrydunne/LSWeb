using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Sqs;

namespace Foundation.Application.Queries.ListSqsSubscriptions;

/// <summary>
/// List the SNS topics that publish to a queue, detected from the queue's access policy.
/// </summary>
/// <param name="QueueName">The name of the queue to inspect.</param>
public record ListSqsSubscriptionsQuery(string QueueName) : IQuery<ListSqsSubscriptionsQueryResult>;

/// <summary>
/// The SNS subscriptions that target a queue.
/// </summary>
/// <param name="Subscriptions">The subscriptions, in first-seen order.</param>
public record ListSqsSubscriptionsQueryResult(IReadOnlyList<SqsQueueSubscription> Subscriptions);
