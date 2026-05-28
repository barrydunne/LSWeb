namespace Foundation.Domain.Sns;

/// <summary>
/// A subscription attached to an SNS topic. Captures the delivery protocol and the target endpoint
/// so the UI can render a cross-resource link to the destination (for example an SQS queue or a
/// Lambda function).
/// </summary>
/// <param name="SubscriptionArn">The Amazon Resource Name of the subscription, or <c>PendingConfirmation</c> when it has not yet been confirmed.</param>
/// <param name="Protocol">The delivery protocol, for example <c>sqs</c>, <c>lambda</c>, or <c>email</c>.</param>
/// <param name="Endpoint">The target endpoint the topic delivers to, typically an ARN for resource protocols.</param>
/// <param name="Owner">The AWS account identifier that owns the subscription.</param>
public sealed record SnsSubscription(
    string SubscriptionArn,
    string Protocol,
    string Endpoint,
    string Owner);
