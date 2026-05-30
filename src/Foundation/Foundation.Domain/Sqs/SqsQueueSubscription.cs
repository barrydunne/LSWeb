namespace Foundation.Domain.Sqs;

/// <summary>
/// An SNS topic that publishes to a queue, detected from the queue's access policy or from the SNS
/// topic subscriptions that target the queue. Lets the UI show the relationship as a cross-resource
/// link to the owning topic.
/// </summary>
/// <param name="TopicArn">The ARN of the SNS topic that is allowed to send to the queue.</param>
/// <param name="TopicName">The bare topic name derived from the ARN.</param>
public sealed record SqsQueueSubscription(string TopicArn, string TopicName);
