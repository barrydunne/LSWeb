using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Sqs;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.GetSqsQueueRedrive;

internal sealed partial class GetSqsQueueRedriveQueryHandler
    : IQueryHandler<GetSqsQueueRedriveQuery, GetSqsQueueRedriveQueryResult>
{
    private readonly ISqsClient _client;
    private readonly ILogger _logger;

    public GetSqsQueueRedriveQueryHandler(ISqsClient client, ILogger<GetSqsQueueRedriveQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<GetSqsQueueRedriveQueryResult>> Handle(
        GetSqsQueueRedriveQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.QueueName);
        var redrive = await _client.GetQueueRedriveAsync(request.QueueName, cancellationToken);
        LogHandled(redrive.IsSuccess);

        if (!redrive.IsSuccess)
        {
            Result<GetSqsQueueRedriveQueryResult> failure = redrive.Error!.Value;
            return failure;
        }

        return new GetSqsQueueRedriveQueryResult(redrive.Value);
    }

    [LoggerMessage(LogLevel.Trace, "Reading redrive relationships for SQS queue {QueueName}.")]
    private partial void LogHandling(string queueName);

    [LoggerMessage(LogLevel.Trace, "SQS queue redrive read handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
