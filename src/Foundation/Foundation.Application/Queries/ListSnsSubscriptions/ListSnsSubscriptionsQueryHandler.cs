using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Sns;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListSnsSubscriptions;

internal sealed partial class ListSnsSubscriptionsQueryHandler
    : IQueryHandler<ListSnsSubscriptionsQuery, ListSnsSubscriptionsQueryResult>
{
    private readonly ISnsClient _client;
    private readonly ILogger _logger;

    public ListSnsSubscriptionsQueryHandler(ISnsClient client, ILogger<ListSnsSubscriptionsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListSnsSubscriptionsQueryResult>> Handle(
        ListSnsSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.TopicArn);
        var subscriptions = await _client.ListSubscriptionsByTopicAsync(request.TopicArn, cancellationToken);
        LogHandled(subscriptions.IsSuccess);

        if (!subscriptions.IsSuccess)
        {
            Result<ListSnsSubscriptionsQueryResult> failure = subscriptions.Error!.Value;
            return failure;
        }

        return new ListSnsSubscriptionsQueryResult(subscriptions.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing SNS subscriptions for topic {TopicArn}.")]
    private partial void LogHandling(string topicArn);

    [LoggerMessage(LogLevel.Trace, "SNS subscription list handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
