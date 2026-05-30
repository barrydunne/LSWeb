using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Sns;
using Foundation.Application.Sqs;
using Foundation.Domain.Navigation;
using Foundation.Domain.Sns;
using Foundation.Domain.Sqs;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListSqsSubscriptions;

internal sealed partial class ListSqsSubscriptionsQueryHandler
    : IQueryHandler<ListSqsSubscriptionsQuery, ListSqsSubscriptionsQueryResult>
{
    private const string SqsServiceNamespace = "sqs";
    private const string SqsProtocol = "sqs";

    private readonly ISqsClient _client;
    private readonly ISnsClient _snsClient;
    private readonly ILogger _logger;

    public ListSqsSubscriptionsQueryHandler(
        ISqsClient client, ISnsClient snsClient, ILogger<ListSqsSubscriptionsQueryHandler> logger)
    {
        _client = client;
        _snsClient = snsClient;
        _logger = logger;
    }

    public async Task<Result<ListSqsSubscriptionsQueryResult>> Handle(
        ListSqsSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.QueueName);

        var policySubscriptions = await _client.GetQueueSubscriptionsAsync(request.QueueName, cancellationToken);
        if (!policySubscriptions.IsSuccess)
        {
            LogHandled(false);
            return policySubscriptions.Error!.Value;
        }

        var topicSubscriptions = await DiscoverTopicSubscriptionsAsync(request.QueueName, cancellationToken);
        if (!topicSubscriptions.IsSuccess)
        {
            LogHandled(false);
            return topicSubscriptions.Error!.Value;
        }

        LogHandled(true);
        var merged = MergeSubscriptions(policySubscriptions.Value, topicSubscriptions.Value);
        return new ListSqsSubscriptionsQueryResult(merged);
    }

    private async Task<Result<IReadOnlyList<SqsQueueSubscription>>> DiscoverTopicSubscriptionsAsync(
        string queueName, CancellationToken cancellationToken)
    {
        var topics = await _snsClient.ListTopicsAsync(cancellationToken);
        if (!topics.IsSuccess)
            return topics.Error!.Value;

        var discovered = new List<SqsQueueSubscription>();
        foreach (var topic in topics.Value)
        {
            var subscriptions = await _snsClient.ListSubscriptionsByTopicAsync(topic.TopicArn, cancellationToken);
            if (!subscriptions.IsSuccess)
                return subscriptions.Error!.Value;

            if (subscriptions.Value.Any(_ => SubscriptionTargetsQueue(_, queueName)))
                discovered.Add(new SqsQueueSubscription(topic.TopicArn, topic.Name));
        }

        return discovered;
    }

    private static bool SubscriptionTargetsQueue(SnsSubscription subscription, string queueName)
    {
        if (!string.Equals(subscription.Protocol, SqsProtocol, StringComparison.OrdinalIgnoreCase))
            return false;

        if (string.IsNullOrEmpty(subscription.Endpoint))
            return false;

        if (ArnParts.TryParse(subscription.Endpoint, out var parts)
            && string.Equals(parts.Service, SqsServiceNamespace, StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(parts.ResourceId, queueName, StringComparison.OrdinalIgnoreCase);
        }

        return string.Equals(subscription.Endpoint, queueName, StringComparison.OrdinalIgnoreCase);
    }

    private static List<SqsQueueSubscription> MergeSubscriptions(
        IReadOnlyList<SqsQueueSubscription> policySubscriptions,
        IReadOnlyList<SqsQueueSubscription> topicSubscriptions)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        return policySubscriptions
              .Concat(topicSubscriptions)
              .Where(_ => seen.Add(_.TopicArn))
              .OrderBy(_ => _.TopicArn, StringComparer.Ordinal)
              .ToList();
    }

    [LoggerMessage(LogLevel.Trace, "Listing SNS subscriptions for SQS queue {QueueName}.")]
    private partial void LogHandling(string queueName);

    [LoggerMessage(LogLevel.Trace, "SQS subscription list handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
