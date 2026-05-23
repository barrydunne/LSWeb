using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Sqs;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListSqsSubscriptions;

internal sealed partial class ListSqsSubscriptionsQueryHandler
    : IQueryHandler<ListSqsSubscriptionsQuery, ListSqsSubscriptionsQueryResult>
{
    private readonly ISqsClient _client;
    private readonly ILogger _logger;

    public ListSqsSubscriptionsQueryHandler(ISqsClient client, ILogger<ListSqsSubscriptionsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListSqsSubscriptionsQueryResult>> Handle(
        ListSqsSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.QueueName);
        var subscriptions = await _client.GetQueueSubscriptionsAsync(request.QueueName, cancellationToken);
        LogHandled(subscriptions.IsSuccess);

        if (!subscriptions.IsSuccess)
        {
            Result<ListSqsSubscriptionsQueryResult> failure = subscriptions.Error!.Value;
            return failure;
        }

        return new ListSqsSubscriptionsQueryResult(subscriptions.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing SNS subscriptions for SQS queue {QueueName}.")]
    private partial void LogHandling(string queueName);

    [LoggerMessage(LogLevel.Trace, "SQS subscription list handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
