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
}
