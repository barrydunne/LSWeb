using AspNet.KickStarter.CQRS.Abstractions.Queries;
using Foundation.Domain.Sns;

namespace Foundation.Application.Queries.ListSnsSubscriptions;

/// <summary>
/// List the subscriptions attached to an SNS topic.
/// </summary>
/// <param name="TopicArn">The Amazon Resource Name of the topic to inspect.</param>
public record ListSnsSubscriptionsQuery(string TopicArn) : IQuery<ListSnsSubscriptionsQueryResult>;

/// <summary>
/// The subscriptions attached to an SNS topic.
/// </summary>
/// <param name="Subscriptions">The subscriptions, ordered as returned by the backend.</param>
public record ListSnsSubscriptionsQueryResult(IReadOnlyList<SnsSubscription> Subscriptions);
