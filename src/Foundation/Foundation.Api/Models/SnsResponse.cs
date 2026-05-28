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
