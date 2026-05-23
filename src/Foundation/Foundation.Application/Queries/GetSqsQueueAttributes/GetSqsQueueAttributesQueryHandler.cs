using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Sqs;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetSqsQueueAttributes;

internal sealed partial class GetSqsQueueAttributesQueryHandler
    : IQueryHandler<GetSqsQueueAttributesQuery, GetSqsQueueAttributesQueryResult>
{
    private readonly ISqsClient _client;
    private readonly ILogger _logger;

    public GetSqsQueueAttributesQueryHandler(ISqsClient client, ILogger<GetSqsQueueAttributesQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetSqsQueueAttributesQueryResult>> Handle(
        GetSqsQueueAttributesQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.QueueName);
        var attributes = await _client.GetQueueAttributesAsync(request.QueueName, cancellationToken);
        LogHandled(attributes.IsSuccess);

        if (!attributes.IsSuccess)
        {
            Result<GetSqsQueueAttributesQueryResult> failure = attributes.Error!.Value;
            return failure;
        }

        return new GetSqsQueueAttributesQueryResult(attributes.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Reading attributes for SQS queue {QueueName}.")]
    private partial void LogHandling(string queueName);

    [LoggerMessage(LogLevel.Trace, "SQS queue attributes read handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
