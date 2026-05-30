using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Sqs;

namespace Foundation.Application.Queries.ListSqsSubscriptions;

/// <summary>
/// List the SNS topics that publish to a queue. Topics are discovered both from the queue's access
/// policy and from the SNS topic subscriptions that target the queue (the authoritative source).
/// </summary>
/// <param name="QueueName">The name of the queue to inspect.</param>
public record ListSqsSubscriptionsQuery(string QueueName) : IQuery<ListSqsSubscriptionsQueryResult>;

/// <summary>
/// The SNS subscriptions that target a queue.
/// </summary>
/// <param name="Subscriptions">The subscriptions, de-duplicated and ordered by topic ARN.</param>
public record ListSqsSubscriptionsQueryResult(IReadOnlyList<SqsQueueSubscription> Subscriptions);
