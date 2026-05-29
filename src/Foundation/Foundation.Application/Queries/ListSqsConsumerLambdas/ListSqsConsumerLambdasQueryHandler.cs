using AspNet.KickStarter.CQRS.Abstractions.Queries;
using AspNet.KickStarter.FunctionalResult;
using Foundation.Application.Lambda;
using Foundation.Domain.Sqs;
using Microsoft.Extensions.Logging;

namespace Foundation.Application.Queries.ListSqsConsumerLambdas;

internal sealed partial class ListSqsConsumerLambdasQueryHandler
    : IQueryHandler<ListSqsConsumerLambdasQuery, ListSqsConsumerLambdasQueryResult>
{
    private readonly ILambdaClient _client;
    private readonly ILogger _logger;

    public ListSqsConsumerLambdasQueryHandler(ILambdaClient client, ILogger<ListSqsConsumerLambdasQueryHandler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result<ListSqsConsumerLambdasQueryResult>> Handle(
        ListSqsConsumerLambdasQuery request, CancellationToken cancellationToken)
    {
        LogHandling(request.QueueName);

        var functions = await _client.ListFunctionsAsync(cancellationToken);
        if (!functions.IsSuccess)
        {
            LogHandled(false);
            Result<ListSqsConsumerLambdasQueryResult> failure = functions.Error!.Value;
            return failure;
        }

        var arnSuffix = $":{request.QueueName}";
        var consumers = new List<SqsConsumerLambda>();
        foreach (var function in functions.Value)
        {
            var mappings = await _client.ListEventSourceMappingsAsync(function.FunctionName, cancellationToken);
            if (!mappings.IsSuccess)
            {
                LogHandled(false);
                Result<ListSqsConsumerLambdasQueryResult> failure = mappings.Error!.Value;
                return failure;
            }

            var match = mappings.Value
                .FirstOrDefault(mapping => mapping.EventSourceArn.EndsWith(arnSuffix, StringComparison.Ordinal));
            if (match is not null)
            {
                consumers.Add(new SqsConsumerLambda(function.FunctionName, match.FunctionArn, match.State));
            }
        }

        var ordered = consumers
            .OrderBy(consumer => consumer.FunctionName, StringComparer.Ordinal)
            .ToList();

        LogHandled(true);
        return new ListSqsConsumerLambdasQueryResult(ordered);
    }

    [LoggerMessage(LogLevel.Trace, "Listing Lambda consumers for SQS queue {QueueName}.")]
    private partial void LogHandling(string queueName);

    [LoggerMessage(LogLevel.Trace, "SQS consumer Lambda list handled. Success: {Success}")]
    private partial void LogHandled(bool success);
}
