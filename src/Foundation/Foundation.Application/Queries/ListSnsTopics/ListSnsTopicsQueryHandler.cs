using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Sns;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListSnsTopics;

internal sealed partial class ListSnsTopicsQueryHandler : IQueryHandler<ListSnsTopicsQuery, ListSnsTopicsQueryResult>
{
    private readonly ISnsClient _client;
    private readonly ILogger _logger;

    public ListSnsTopicsQueryHandler(ISnsClient client, ILogger<ListSnsTopicsQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListSnsTopicsQueryResult>> Handle(ListSnsTopicsQuery request, CancellationToken cancellationToken)
    {
        LogHandling();
        var topics = await _client.ListTopicsAsync(cancellationToken);
        LogHandled(topics.IsSuccess);

        if (!topics.IsSuccess)
        {
            Result<ListSnsTopicsQueryResult> failure = topics.Error!.Value;
            return failure;
        }

        return new ListSnsTopicsQueryResult(topics.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Listing SNS topics.")]
    private partial void LogHandling();

    [LoggerMessage(LogLevel.Trace, "SNS topic list handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
