using AspNet.KickStarter.FunctionalResult;
using Foundation.Domain.Sns;

namespace Foundation.Application.Sns;

/// <summary>
/// Abstracts the SNS operations the application needs so the handlers stay free of any direct AWS
/// SDK dependency. The implementation flows every call through the resilient AWS gateway and
/// translates failures into a <see cref="Result"/> rather than throwing.
/// </summary>
public interface ISnsClient
{
    /// <summary>
    /// List the SNS topics available on the backend.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The topics, or an error when the backend cannot be reached.</returns>
    Task<Result<IReadOnlyList<SnsTopic>>> ListTopicsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Create an SNS topic with the supplied specification.
    /// </summary>
    /// <param name="specification">The topic to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A successful result, or an error when the backend rejects the request.</returns>
    Task<Result> CreateTopicAsync(
        SnsTopicSpecification specification, CancellationToken cancellationToken);

    /// <summary>
    /// Delete an SNS topic by its Amazon Resource Name.
    /// </summary>
    /// <param name="topicArn">The Amazon Resource Name of the topic to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A successful result, or an error when the backend rejects the request.</returns>
    Task<Result> DeleteTopicAsync(string topicArn, CancellationToken cancellationToken);

    /// <summary>
    /// List the subscriptions attached to an SNS topic.
    /// </summary>
    /// <param name="topicArn">The Amazon Resource Name of the topic to inspect.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The subscriptions, or an error when the backend cannot be reached.</returns>
    Task<Result<IReadOnlyList<SnsSubscription>>> ListSubscriptionsByTopicAsync(
        string topicArn, CancellationToken cancellationToken);

    /// <summary>
    /// Subscribe an endpoint to an SNS topic using the supplied protocol.
    /// </summary>
    /// <param name="topicArn">The Amazon Resource Name of the topic to subscribe to.</param>
    /// <param name="protocol">The delivery protocol, for example <c>sqs</c>, <c>lambda</c>, or <c>email</c>.</param>
    /// <param name="endpoint">The endpoint to deliver to (an ARN for resource protocols, an address for email/SMS).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A successful result, or an error when the backend rejects the request.</returns>
    Task<Result> SubscribeAsync(
        string topicArn, string protocol, string endpoint, CancellationToken cancellationToken);

    /// <summary>
    /// Remove a subscription from an SNS topic by its Amazon Resource Name.
    /// </summary>
    /// <param name="subscriptionArn">The Amazon Resource Name of the subscription to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A successful result, or an error when the backend rejects the request.</returns>
    Task<Result> UnsubscribeAsync(string subscriptionArn, CancellationToken cancellationToken);

    /// <summary>
    /// Publish a message to an SNS topic, optionally with a subject and custom string attributes.
    /// </summary>
    /// <param name="topicArn">The Amazon Resource Name of the topic to publish to.</param>
    /// <param name="subject">The optional subject; ignored when null or empty.</param>
    /// <param name="message">The message body.</param>
    /// <param name="messageAttributes">Custom string message attributes to attach to the message.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A successful result, or an error when the backend rejects the request.</returns>
    Task<Result> PublishAsync(
        string topicArn,
        string? subject,
        string message,
        IReadOnlyDictionary<string, string> messageAttributes,
        CancellationToken cancellationToken);

    /// <summary>
    /// Get the filter policy attached to an SNS subscription.
    /// </summary>
    /// <param name="subscriptionArn">The Amazon Resource Name of the subscription to inspect.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The filter policy as a JSON document, an empty string when no policy is set, or an error when the backend cannot be reached.</returns>
    Task<Result<string>> GetSubscriptionFilterPolicyAsync(
        string subscriptionArn, CancellationToken cancellationToken);

    /// <summary>
    /// Set or clear the filter policy attached to an SNS subscription.
    /// </summary>
    /// <param name="subscriptionArn">The Amazon Resource Name of the subscription to update.</param>
    /// <param name="filterPolicy">The filter policy as a JSON document; an empty string clears the policy.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A successful result, or an error when the backend rejects the request.</returns>
    Task<Result> SetSubscriptionFilterPolicyAsync(
        string subscriptionArn, string filterPolicy, CancellationToken cancellationToken);
}
