namespace Foundation.Api.Models;

/// <summary>
/// The SNS topics available on the backend.
/// </summary>
/// <param name="Topics">The topic summaries, ordered as returned by the backend.</param>
public sealed record SnsTopicListResponse(
    IReadOnlyList<SnsTopicSummaryResponse> Topics);

/// <summary>
/// A concise view of an SNS topic as it appears in a topic list.
/// </summary>
/// <param name="Name">The topic name derived from its Amazon Resource Name.</param>
/// <param name="TopicArn">The Amazon Resource Name that uniquely identifies the topic.</param>
public sealed record SnsTopicSummaryResponse(
    string Name,
    string TopicArn);

/// <summary>
/// The details required to create an SNS topic.
/// </summary>
/// <param name="Name">The name of the topic to create.</param>
public sealed record SnsTopicCreateRequest(string Name);

/// <summary>
/// The subscriptions attached to an SNS topic.
/// </summary>
/// <param name="Subscriptions">The subscription summaries, ordered as returned by the backend.</param>
public sealed record SnsSubscriptionListResponse(
    IReadOnlyList<SnsSubscriptionSummaryResponse> Subscriptions);

/// <summary>
/// A concise view of a single SNS subscription.
/// </summary>
/// <param name="SubscriptionArn">The Amazon Resource Name of the subscription, or <c>PendingConfirmation</c> when unconfirmed.</param>
/// <param name="Protocol">The delivery protocol, for example <c>sqs</c>, <c>lambda</c>, or <c>email</c>.</param>
/// <param name="Endpoint">The target endpoint the topic delivers to.</param>
/// <param name="Owner">The AWS account identifier that owns the subscription.</param>
public sealed record SnsSubscriptionSummaryResponse(
    string SubscriptionArn,
    string Protocol,
    string Endpoint,
    string Owner);

/// <summary>
/// A request to publish a message to an SNS topic.
/// </summary>
/// <param name="TopicArn">The Amazon Resource Name of the topic to publish to.</param>
/// <param name="Subject">The optional subject; ignored when null or empty.</param>
/// <param name="Message">The message body.</param>
/// <param name="MessageAttributes">Custom string message attributes to attach to the message.</param>
public sealed record SnsPublishMessageRequest(
    string TopicArn,
    string? Subject,
    string Message,
    IReadOnlyDictionary<string, string>? MessageAttributes);

/// <summary>
/// The filter policy attached to an SNS subscription.
/// </summary>
/// <param name="FilterPolicy">The filter policy as a JSON document, or an empty string when no policy is set.</param>
public sealed record SnsSubscriptionFilterPolicyResponse(string FilterPolicy);

/// <summary>
/// A request to set or clear the filter policy attached to an SNS subscription.
/// </summary>
/// <param name="SubscriptionArn">The Amazon Resource Name of the subscription to update.</param>
/// <param name="FilterPolicy">The filter policy as a JSON document; an empty string clears the policy.</param>
public sealed record SnsSubscriptionFilterPolicyRequest(
    string SubscriptionArn,
    string FilterPolicy);

/// <summary>
/// A request to subscribe an endpoint to an SNS topic using the supplied protocol.
/// </summary>
/// <param name="TopicArn">The Amazon Resource Name of the topic to subscribe to.</param>
/// <param name="Protocol">The delivery protocol, for example <c>sqs</c>, <c>lambda</c>, or <c>email</c>.</param>
/// <param name="Endpoint">The endpoint to deliver to.</param>
public sealed record SnsSubscribeRequest(
    string TopicArn,
    string Protocol,
    string Endpoint);
