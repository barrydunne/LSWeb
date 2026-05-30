using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.SetSnsSubscriptionFilterPolicy;

/// <summary>
/// Set or clear the filter policy attached to an SNS subscription.
/// </summary>
/// <param name="SubscriptionArn">The Amazon Resource Name of the subscription to update.</param>
/// <param name="FilterPolicy">The filter policy as a JSON document; an empty string clears the policy.</param>
public record SetSnsSubscriptionFilterPolicyCommand(
    string SubscriptionArn,
    string FilterPolicy) : ICommand;
