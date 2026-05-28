using System.Diagnostics.CodeAnalysis;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Sns;
using Foundation.Domain.Sns;
using Foundation.Infrastructure.Aws;

namespace Foundation.Infrastructure.Sns;

/// <summary>
/// Reads and writes SNS through the resilient AWS gateway so the same code works against LocalStack
/// or real AWS. All access flows through <see cref="IAwsGateway"/>, which records capability and
/// converts failures into a <see cref="Result{T}"/> rather than throwing.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Tested with integration tests.")]
internal sealed class SnsClientAdapter : ISnsClient
{
    private const string ServiceKey = "sns";

    private readonly IAwsGateway _gateway;

    public SnsClientAdapter(IAwsGateway gateway)
        => _gateway = gateway;

    public Task<Result<IReadOnlyList<SnsTopic>>> ListTopicsAsync(CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonSimpleNotificationServiceClient, IReadOnlyList<SnsTopic>>(
            ServiceKey,
            async (client, token) =>
            {
                var topics = new List<SnsTopic>();
                string? nextToken = null;

                do
                {
                    var response = await client.ListTopicsAsync(
                        new ListTopicsRequest { NextToken = nextToken },
                        token);

                    foreach (var topic in response.Topics ?? [])
                        topics.Add(ToTopic(topic.TopicArn ?? string.Empty));

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return topics;
            },
            cancellationToken);

    public async Task<Result> CreateTopicAsync(
        SnsTopicSpecification specification, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonSimpleNotificationServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.CreateTopicAsync(
                    new CreateTopicRequest { Name = specification.Name },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public async Task<Result> DeleteTopicAsync(string topicArn, CancellationToken cancellationToken)
    {
        var result = await _gateway.ExecuteAsync<AmazonSimpleNotificationServiceClient, bool>(
            ServiceKey,
            async (client, token) =>
            {
                await client.DeleteTopicAsync(
                    new DeleteTopicRequest { TopicArn = topicArn },
                    token);
                return true;
            },
            cancellationToken);

        return result.IsSuccess ? Result.Success() : result.Error!.Value;
    }

    public Task<Result<IReadOnlyList<SnsSubscription>>> ListSubscriptionsByTopicAsync(
        string topicArn, CancellationToken cancellationToken)
        => _gateway.ExecuteAsync<AmazonSimpleNotificationServiceClient, IReadOnlyList<SnsSubscription>>(
            ServiceKey,
            async (client, token) =>
            {
                var subscriptions = new List<SnsSubscription>();
                string? nextToken = null;

                do
                {
                    var response = await client.ListSubscriptionsByTopicAsync(
                        new ListSubscriptionsByTopicRequest { TopicArn = topicArn, NextToken = nextToken },
                        token);

                    foreach (var subscription in response.Subscriptions ?? [])
                        subscriptions.Add(ToSubscription(subscription));

                    nextToken = response.NextToken;
                }
                while (!string.IsNullOrEmpty(nextToken));

                return subscriptions;
            },
            cancellationToken);

    private static SnsSubscription ToSubscription(Subscription subscription)
        => new(
            subscription.SubscriptionArn ?? string.Empty,
            subscription.Protocol ?? string.Empty,
            subscription.Endpoint ?? string.Empty,
            subscription.Owner ?? string.Empty);

    private static SnsTopic ToTopic(string topicArn)
    {
        var separatorIndex = topicArn.LastIndexOf(':');
        var name = separatorIndex >= 0 && separatorIndex < topicArn.Length - 1
            ? topicArn[(separatorIndex + 1)..]
            : topicArn;
        return new SnsTopic(name, topicArn);
    }
}
