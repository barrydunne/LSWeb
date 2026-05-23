using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Sqs;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListSqsQueues;

internal sealed partial class ListSqsQueuesQueryHandler : IQueryHandler<ListSqsQueuesQuery, ListSqsQueuesQueryResult>
{
    private readonly ISqsClient _client;
    private readonly ILogger _logger;

    public ListSqsQueuesQueryHandler(ISqsClient client, ILogger<ListSqsQueuesQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListSqsQueuesQueryResult>> Handle(ListSqsQueuesQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var queues = await _client.ListQueuesAsync(cancellationToken);
        LogHandled(queues.IsSuccess);

        if (!queues.IsSuccess)
        {
            Result<ListSqsQueuesQueryResult> failure = queues.Error!.Value;
            return failure;
        }

        return new ListSqsQueuesQueryResult(queues.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing SQS queues.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "SQS queue listing handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
