using AspNet.KickStarter.CQRS.Abstractions.Queries;

namespace Foundation.Application.Queries.GetSnsSubscriptionFilterPolicy;

/// <summary>
/// Get the filter policy attached to an SNS subscription.
/// </summary>
/// <param name="SubscriptionArn">The Amazon Resource Name of the subscription to inspect.</param>
public record GetSnsSubscriptionFilterPolicyQuery(string SubscriptionArn)
    : IQuery<GetSnsSubscriptionFilterPolicyQueryResult>;

/// <summary>
/// The filter policy attached to an SNS subscription.
/// </summary>
/// <param name="FilterPolicy">The filter policy as a JSON document, or an empty string when no policy is set.</param>
public record GetSnsSubscriptionFilterPolicyQueryResult(string FilterPolicy);
