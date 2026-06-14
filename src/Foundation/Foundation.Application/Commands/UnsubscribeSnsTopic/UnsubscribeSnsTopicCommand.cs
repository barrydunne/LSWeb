using AspNet.KickStarter.CQRS.Abstractions.Commands;

namespace Foundation.Application.Commands.UnsubscribeSnsTopic;

/// <summary>
/// Remove a subscription from an SNS topic by its Amazon Resource Name.
/// </summary>
/// <param name="SubscriptionArn">The Amazon Resource Name of the subscription to remove.</param>
public record UnsubscribeSnsTopicCommand(string SubscriptionArn) : ICommand;
