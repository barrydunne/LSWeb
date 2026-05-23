using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Sqs;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListSqsMessages;

internal sealed partial class ListSqsMessagesQueryHandler
    : IQueryHandler<ListSqsMessagesQuery, ListSqsMessagesQueryResult>
{
    private readonly ISqsClient _client;
    private readonly ILogger _logger;

    public ListSqsMessagesQueryHandler(ISqsClient client, ILogger<ListSqsMessagesQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListSqsMessagesQueryResult>> Handle(
        ListSqsMessagesQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.QueueName, request.Mode);
        var messages = await _client.ReceiveMessagesAsync(
            request.QueueName, request.Mode, request.MaxMessages, cancellationToken);
        LogHandled(messages.IsSuccess);

        if (!messages.IsSuccess)
        {
            Result<ListSqsMessagesQueryResult> failure = messages.Error!.Value;
            return failure;
        }

        return new ListSqsMessagesQueryResult(messages.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Polling SQS queue {QueueName} in {Mode} mode.")]
    private partial void LogHandling(string queueName, SqsPollMode mode);

    [LoggerMessage(LogLevel.Trace, "SQS message poll handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
